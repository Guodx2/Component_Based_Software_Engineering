import React from 'react';

interface Notification {
  id: number;
  mentorName: string;
  mentorLastName: string;
  requestDate: string;
  message?: string;
  rejectionReason?: string;
}

interface NotificationModalProps {
  notifications: Notification[];
  onClose: () => void;
}

const NotificationModal: React.FC<NotificationModalProps> = ({ notifications, onClose }) => {
  return (
    <div className="notification-overlay" onClick={onClose}>
      <div className="notification-modal" onClick={e => e.stopPropagation()}>
        <div className="notification-arrow"></div>
        <h3>Pranešimai</h3>
        {notifications.length === 0 ? (
          <p>Nėra naujų pranešimų.</p>
        ) : (
          <ul>
            {notifications.map(n => (
              <li key={n.id}>
                {n.message === "atmestas" ? (
                  <>
                    Jūsų prašymas buvo <strong>atmestas</strong> mentoriaus {n.mentorName} {n.mentorLastName} –{" "}
                    {new Date(n.requestDate).toLocaleString()}
                    {n.rejectionReason && (
                      <>. <br></br><br></br><strong>Priežastis:</strong> 
                      <br></br>{n.rejectionReason}</>
                    )}
                  </>
                ) : (
                  <>
                    Jūsų prašymas buvo <strong>patvirtintas</strong> mentoriaus {n.mentorName} {n.mentorLastName} –{" "}
                    {new Date(n.requestDate).toLocaleString()}
                  </>
                )}
              </li>
            ))}
          </ul>
        )}
        <button className="notification-close-btn" onClick={onClose}>Uždaryti</button>
      </div>
    </div>
  );
};

export default NotificationModal;