import React, { useState, useEffect } from 'react';
import './FeedbackModal.scss';

interface FeedbackModalProps {
  semesterPlanTaskId: number;
  onClose: () => void;
  onFeedbackSubmitted: () => void;
  isEdit?: boolean;
  initialRating?: number;
  initialComment?: string;
}

const FeedbackModal: React.FC<FeedbackModalProps> = ({ 
  semesterPlanTaskId, 
  onClose, 
  onFeedbackSubmitted,
  isEdit = false,
  initialRating = 1,
  initialComment = ''
}) => {
  const [rating, setRating] = useState<number>(initialRating);
  const [comment, setComment] = useState<string>(initialComment);

  // If props change (when modal is opened in edit mode), update state.
  useEffect(() => {
    setRating(initialRating);
    setComment(initialComment);
  }, [initialRating, initialComment]);

  const handleSubmit = () => {
    const payload = {
      SemesterPlanTaskId: semesterPlanTaskId,
      Rating: rating,
      Comment: comment
    };

    const url = isEdit 
      ? `http://localhost:5216/api/feedback/update/${semesterPlanTaskId}`
      : `http://localhost:5216/api/feedback/create`;

    const method = isEdit ? 'PUT' : 'POST';

    fetch(url, {
      method: method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
      .then(res => {
        if (res.ok) {
          onFeedbackSubmitted();
          onClose();
        } else {
          console.error("Error submitting feedback.");
        }
      })
      .catch(err => console.error("Error:", err));
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="feedback-modal-content" onClick={e => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>&times;</button>
        <h3>{ isEdit ? "Redaguokite atsiliepimą" : "Palikite atsiliepimą" }</h3>
        <div>
          <label>Įvertinimas (1-5): </label>
          <select value={rating} onChange={(e) => setRating(Number(e.target.value))}>
            <option value={1}>1</option>
            <option value={2}>2</option>
            <option value={3}>3</option>
            <option value={4}>4</option>
            <option value={5}>5</option>
          </select>
        </div>
        <div>
          <label>Komentaras:</label>
        </div>
        <div>
          <textarea 
            value={comment} 
            onChange={(e) => setComment(e.target.value)} 
            placeholder="Įveskite savo komentarą"
          />
        </div>
        <button className="btn-primary" onClick={handleSubmit}>
          { isEdit ? "Atnaujinti atsiliepimą" : "Siųsti atsiliepimą" }
        </button>
      </div>
    </div>
  );
};

export default FeedbackModal;