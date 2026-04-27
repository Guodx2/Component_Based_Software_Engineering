import React, { useState } from 'react';
import { format } from 'date-fns';
import DatePicker, { registerLocale } from 'react-datepicker';
import {lt} from 'date-fns/locale/lt';
import "react-datepicker/dist/react-datepicker.css";
import './CreateTrainingModal.scss';

registerLocale('lt', lt);

interface EditAffairModalProps {
  affair: {
    affairId: number; // Ensure your GET returns this field
    name: string;
    startTime: string;
    endTime: string;
    address: string;
    comment: string;
  };
  onClose: () => void;
  onAffairUpdated: () => void;
}

const EditAffairModal: React.FC<EditAffairModalProps> = ({ affair, onClose, onAffairUpdated }) => {
  const [name, setName] = useState(affair.name);
  const [startDate, setStartDate] = useState<Date | null>(new Date(affair.startTime));
  const [endDate, setEndDate] = useState<Date | null>(new Date(affair.endTime));
  const [address, setAddress] = useState(affair.address);
  const [comment, setComment] = useState(affair.comment);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!startDate || !endDate) {
      alert("Pasirinkite pradžios ir pabaigos laiką.");
      return;
    }

    if (endDate <= startDate) {
      alert("Pabaigos laikas turėtų būti vėlesnis už pradžios laiką.");
      return;
    }

    const formattedStart = format(startDate, "yyyy-MM-dd'T'HH:mm:ss");
    const formattedEnd = format(endDate, "yyyy-MM-dd'T'HH:mm:ss");

    const payload = {
      name,
      startTime: formattedStart,
      endTime: formattedEnd,
      address,
      comment
    };

    const response = await fetch(`http://localhost:5216/api/events/affairs/${affair.affairId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      onAffairUpdated();
      onClose();
    } else {
      console.error('Error updating affair');
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <button className="close-button" onClick={onClose}>
          <span className="close-icon">&times;</span>
        </button>
        <h2>Redaguoti renginį</h2>
        <form onSubmit={handleSubmit}>
          <label>Pavadinimas</label>
          <input 
            type="text" 
            value={name} 
            onChange={e => setName(e.target.value)} 
            required 
            maxLength={20}
          />

          <label>Pradžios laikas</label>
          <DatePicker
            className="small-datepicker"
            selected={startDate}
            onChange={date => setStartDate(date)}
            showTimeSelect
            timeFormat="HH:mm"
            timeIntervals={15}
            dateFormat="Pp"
            locale="lt"
            timeCaption="Laikas"
            placeholderText="Pasirinkite pradžios laiką"
            required
          />

          <label>Pabaigos laikas</label>
          <DatePicker
            className="small-datepicker"
            selected={endDate}
            onChange={date => setEndDate(date)}
            showTimeSelect
            timeFormat="HH:mm"
            timeIntervals={15}
            dateFormat="Pp"
            locale="lt"
            timeCaption="Laikas"
            placeholderText="Pasirinkite pabaigos laiką"
            required
          />

          <label>Adresas</label>
          <input 
            type="text" 
            value={address} 
            onChange={e => setAddress(e.target.value)} 
            required 
            maxLength={50}
          />

          <label>
            Komentaras:
            <textarea 
                value={comment} 
                onChange={e => setComment(e.target.value)} 
                placeholder="Įveskite komentarą"
            />
          </label>

          <div className="modal-actions">
            <button className="btn btn-success" type="submit">Išsaugoti</button>
            <button className="btn btn-danger" type="button" onClick={onClose}>Atšaukti</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditAffairModal;