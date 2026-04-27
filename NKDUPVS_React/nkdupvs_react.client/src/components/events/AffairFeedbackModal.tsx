import React, { useState } from 'react';

interface AffairFeedbackModalProps {
  affair: {
    affairId: number;
    eventId: number;
    name: string;
    startTime: string;
    endTime: string;
    address: string;
    comment: string;
  };
  onClose: () => void;
  onFeedbackSubmitted: () => void;
}

const AffairFeedbackModal: React.FC<AffairFeedbackModalProps> = ({ affair, onClose, onFeedbackSubmitted }) => {
  const [rating, setRating] = useState<number>(5);
  const [comment, setComment] = useState<string>('');

  const handleSubmit = async () => {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const payload = {
      AffairId: affair.affairId,
      Rating: rating,
      Comment: comment,
      MenteeCode: user.code  // include the logged-in mentee's code
    };
    try {
      const res = await fetch('http://localhost:5216/api/feedback/affair/create', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });
      if (res.ok) {
        alert("Atsiliepimas išsaugotas.");
        onFeedbackSubmitted();
        onClose();
      } else {
        const errText = await res.text();
        alert("Klaida: " + errText);
      }
    } catch (err) {
      console.error("Error submitting feedback:", err);
      alert("Klaida siunčiant atsiliepimą");
    }
  };

  return (
    <div style={{
      position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
      backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex',
      justifyContent: 'center', alignItems: 'center', zIndex: 1000
    }}>
      <div style={{
        backgroundColor: '#fff', padding: '20px', borderRadius: '8px',
        maxWidth: '500px', width: '90%'
      }}>
        <h3>Palikti atsiliepimą: {affair.name}</h3>
        <div>
          <label>Įvertinimas (1-5): </label>
          <input 
            type="number" 
            min="1" 
            max="5" 
            value={rating}
            onChange={(e) => setRating(Number(e.target.value))}
          />
        </div>
        <div style={{ marginTop: '10px' }}>
          <label>Komentaras:</label>
          <textarea
            style={{ width: '100%' }}
            value={comment}
            onChange={(e) => setComment(e.target.value)}
          />
        </div>
        <div style={{ marginTop: '20px', textAlign: 'right' }}>
          <button className="btn btn-primary" onClick={handleSubmit}>
            Siųsti atsiliepimą
          </button>
          <button className="btn btn-secondary" style={{ marginLeft: '10px' }} onClick={onClose}>
            Uždaryti
          </button>
        </div>
      </div>
    </div>
  );
};

export default AffairFeedbackModal;