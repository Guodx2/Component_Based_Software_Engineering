import React, { useState } from 'react';
import './CreateTrainingModal.scss';
import DatePicker, { registerLocale } from 'react-datepicker';
import { format } from 'date-fns';
import {lt} from 'date-fns/locale/lt';
import "react-datepicker/dist/react-datepicker.css";

registerLocale('lt', lt);

interface CreateTrainingModalProps {
  onClose: () => void;
  onTrainingCreated: () => void;
}

const CreateTrainingModal: React.FC<CreateTrainingModalProps> = ({ onClose, onTrainingCreated }) => {
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState<Date | null>(null);
  const [endDate, setEndDate] = useState<Date | null>(null);
  const [address, setAddress] = useState('');
  const [comment, setComment] = useState('');

  const user = JSON.parse(localStorage.getItem('user') || '{}');
  const createdBy = user.code || '';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!startDate || !endDate) {
        alert("Pasirinkite ir pradžios ir pabaigos laiką.");
        return;
    }

    if (endDate <= startDate) {
        alert("Pabaigos laikas turėtų būti vėliau nei pradžios laikas.");
        return;
    }

    // Format dates as local datetime strings (without converting to UTC)
    const formattedStart = format(startDate, "yyyy-MM-dd'T'HH:mm:ss");
    const formattedEnd = format(endDate, "yyyy-MM-dd'T'HH:mm:ss");

    const payload = {
      name,
      startTime: formattedStart,
      endTime: formattedEnd,
      address,
      createdBy,
      comment
    };

    const response = await fetch('http://localhost:5216/api/events/trainings', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      onTrainingCreated();
      onClose();
    } else {
      console.error('Error creating training');
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <button className="close-button" onClick={onClose}>
          <span className="close-icon">&times;</span>
        </button>
        <h2>Sukurti mokymus</h2>
        <form onSubmit={handleSubmit}>
          <label>Pavadinimas</label>
          <input type="text" value={name} onChange={e => setName(e.target.value)} required maxLength={20}/>

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
          <input type="text" value={address} onChange={e => setAddress(e.target.value)} required maxLength={50}/>

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

export default CreateTrainingModal;