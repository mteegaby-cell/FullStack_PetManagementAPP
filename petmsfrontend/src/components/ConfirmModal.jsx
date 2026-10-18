import { Button, Modal, Spinner } from "react-bootstrap";

export default function ConfirmModal({ show, title, message, busy, onConfirm, onCancel }) {
  return (
    <Modal show={show} onHide={onCancel} centered>
      <Modal.Header closeButton><Modal.Title>{title}</Modal.Title></Modal.Header>
      <Modal.Body>{message}</Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onCancel} disabled={busy}>Cancel</Button>
        <Button variant="danger" onClick={onConfirm} disabled={busy}>
          {busy ? <Spinner size="sm" /> : "Delete"}
        </Button>
      </Modal.Footer>
    </Modal>
  );
}
