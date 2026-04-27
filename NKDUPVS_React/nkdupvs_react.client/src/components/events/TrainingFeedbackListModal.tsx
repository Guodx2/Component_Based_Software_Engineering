import React from 'react';

interface Feedback {
    id: number;
    rating: number;
    comment: string;
    submissionDate: string;
    menteeName: string;
    menteeLastName: string;
}

interface TrainingFeedbackListModalProps {
    trainingName: string;
    feedbacks: Feedback[];
    onClose: () => void;
}

// Helper function to format a date string in "YYYY-MM-DD HH:mm" format.
const formatDate = (dateString: string): string => {
    const d = new Date(dateString + "Z");
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    const hours = String(d.getHours()).padStart(2, "0");
    const minutes = String(d.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day} ${hours}:${minutes}`;
};

const TrainingFeedbackListModal: React.FC<TrainingFeedbackListModalProps> = ({ trainingName, feedbacks, onClose }) => {
    return (
        <div style={{
            position: 'fixed', top: 0, left: 0,
            right: 0, bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            zIndex: 1000
        }}>
            <div style={{
                backgroundColor: '#fff', padding: '20px', borderRadius: '8px',
                maxWidth: '600px', width: '90%'
            }}>
                <h3>{trainingName} atsiliepimai</h3>
                {feedbacks.length === 0 ? (
                    <p>Nėra atsiliepimų</p>
                ) : (
                    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                        <thead>
                            <tr>
                                <th style={{ border: '1px solid #ccc', padding: '5px' }}>Komentaras</th>
                                <th style={{ border: '1px solid #ccc', padding: '5px' }}>Data</th>
                                <th style={{ border: '1px solid #ccc', padding: '5px' }}>Ugdytinis</th>
                            </tr>
                        </thead>
                        <tbody>
                            {feedbacks.map(f => (
                                <tr key={f.id}>
                                    <td style={{ border: '1px solid #ccc', padding: '5px' }}>{f.comment}</td>
                                    <td style={{ border: '1px solid #ccc', padding: '5px' }}>
                                        {formatDate(f.submissionDate)}
                                    </td>
                                    <td style={{ border: '1px solid #ccc', padding: '5px' }}>
                                        {f.menteeName} {f.menteeLastName}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
                <button className="btn btn-primary" style={{ marginTop: '10px', float: 'right' }} onClick={onClose}>
                    Uždaryti
                </button>
            </div>
        </div>
    );
};

export default TrainingFeedbackListModal;