import React,{useState,useEffect, Fragment} from "react";
import Table from 'react-bootstrap/Table';

const PETMSCRUD = () => 
{
    const studentData = [
    {
        id: 1,
        Name: "Hlumisa",
        StudentNumber: "s24987654321",
        PhoneNumber: "0617894561"
    },
    {
        id: 2,
        Name: "Lawaat",
        StudentNumber: "s24987654322",
        PhoneNumber: "0845824561"
    }
]   
const [data, setData] = useState([])
useEffect(()=>
{
    setData(studentData);
},[]
)
    return(
        <Fragment>
            <Table striped bordered hover>
                <thead>
                    <tr>
                    <th>#</th>
                    <th>First Name</th>
                    <th>Last Name</th>
                    <th>Username</th>
                    </tr>
                </thead>

                <tbody>
                {
                    data && data.length > 0 ?
                    data.map((item, index)=> 
                        {
                            return
                            ( <tr key = {index}>
                            <td>{index + 1}</td>
                            <td>{item.id}</td>
                            <td>{item.Name}</td>
                            <td>{item.StudentNumber}</td>
                            <td>{item.PhoneNumber}</td>
                            </tr>
                            )
                        })
                        :
                        "Loading..."
                }  
                </tbody>
                </Table>
        </Fragment>
    )
}

export default PETMSCRUD;