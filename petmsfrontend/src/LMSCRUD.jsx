import React, { Fragment, useState, useEffect } from "react";
import Table from "react-bootstrap/Table";
import Form from "react-bootstrap/Form";
import Alert from "react-bootstrap/Alert";
import axios from "axios";
import { Link } from "react-router-dom";

const API = "https://localhost:7242/api/Groomer";
const STATUSES = ["Scheduled", "CheckedIn", "InProgress", "Completed", "Cancelled"];

const LMSCRUD = () => {
    const [data, setData] = useState([]);
    const [filter, setFilter] = useState("");
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    const load = () => {
        axios
            .get(`${API}/appointments`, { params: filter ? { status: filter } : {} })
            .then((res) => setData(res.data))
            .catch(() => setError("Could not load appointments."));
    };

    useEffect(() => {
        load();
    }, [filter]);

    const changeStatus = (id, status) => {
        setError("");
        setMessage("");
        axios
            .put(`${API}/appointments/${id}/status`, { status })
            .then(() => {
                setMessage("Status updated.");
                load();
            })
            .catch(() => setError("Could not update the status."));
    };

    return (
        <Fragment>
            {error && <Alert variant="danger">{error}</Alert>}
            {message && <Alert variant="success">{message}</Alert>}

            <Form.Select
                className="mb-3 w-auto"
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
            >
                <option value="">All statuses</option>
                {STATUSES.map((s) => (
                    <option key={s} value={s}>{s}</option>
                ))}
            </Form.Select>

            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th>#</th><th>Date</th><th>Time</th><th>Pet</th>
                        <th>Owner</th><th>Service</th><th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    {data.length > 0 ? (
                        data.map((item, index) => (
                            <tr key={item.id}>
                                <td>{index + 1}</td>
                                <td>{item.appointmentDate.slice(0, 10)}</td>
                                <td>{item.appointmentTime.slice(0, 5)}</td>
                                <td><Link to={`/pets/${item.petId}`}>{item.petName}</Link></td>
                                <td>{item.ownerName}</td>
                                <td>{item.serviceName}</td>
                                <td>
                                    <Form.Select
                                        size="sm"
                                        value={item.status}
                                        onChange={(e) => changeStatus(item.id, e.target.value)}
                                    >
                                        {STATUSES.map((s) => (
                                            <option key={s} value={s}>{s}</option>
                                        ))}
                                    </Form.Select>
                                </td>
                            </tr>
                        ))
                    ) : (
                        <tr><td colSpan="7">No appointments found.</td></tr>
                    )}
                </tbody>
            </Table>
        </Fragment>
    );
};

export default LMSCRUD;