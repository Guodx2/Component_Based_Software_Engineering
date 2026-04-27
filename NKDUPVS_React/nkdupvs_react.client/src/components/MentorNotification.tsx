import React, { useState, useEffect } from 'react';

interface MentorNotificationProps {
  mentorCode: string;
}

const MentorNotification: React.FC<MentorNotificationProps> = ({ mentorCode }) => {
  const [pendingCount, setPendingCount] = useState<number>(0);
  const [visible, setVisible] = useState<boolean>(true);

  useEffect(() => {
    const fetchRequests = async () => {
      try {
        const res = await fetch(`http://localhost:5216/api/mentor/requests/${mentorCode}`);
        const data = await res.json();
        // Count only requests with status "Laukiama patvirtinimo"
        const count = data.filter((r: any) => r.status === "Laukiama patvirtinimo").length;
        setPendingCount(count);
      } catch (err) {
        console.error(err);
      }
    };

    fetchRequests();
    const intervalId = setInterval(fetchRequests, 60000);
    return () => clearInterval(intervalId);
  }, [mentorCode]);

  if (pendingCount === 0 || !visible) return null;

  return (
    <div style={{
      backgroundColor: '#ffc107',
      color: '#333',
      padding: '15px 20px',
      boxSizing: 'border-box',
      position: 'fixed',
      bottom: '50px',
      right: '20px',
      zIndex: 1000,
      borderRadius: '8px',
      boxShadow: '0 4px 8px rgba(0,0,0,0.2)',
      maxWidth: '300px'
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <p style={{ margin: 0, fontSize: '0.95rem', flex: 1 }}>
          Turite {pendingCount} laukianči{pendingCount > 1 ? 'ų' : 'o'} patvirtinimo ugdytini{pendingCount > 1 ? 'ų' : 'o'} užklausą. Prašome patikrinti&nbsp;
          <a href="/mymentees" style={{ textDecoration: 'underline', color: '#333' }}>Mano ugdytiniai</a>&nbsp;puslapį.
        </p>
        <button 
          onClick={() => setVisible(false)}
          style={{
            background: 'transparent',
            border: 'none',
            fontSize: '1.2rem',
            fontWeight: 'bold',
            cursor: 'pointer',
            marginLeft: '10px'
          }}
          aria-label="Close notification"
        >
          &times;
        </button>
      </div>
    </div>
  );
};

export default MentorNotification;