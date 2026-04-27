import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import './MyMentees.scss';
import SemesterPlanModal from './SemesterPlanModal';
import MenteeTimetableModal from './MenteeTimetableModal';

interface MentorRequestExtended {
  id: number;
  menteeCode: string;
  mentorCode: string;
  requestDate: string;
  status: string;
  name: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  studyProgram: string; 
  specialization: string;
}

interface MenteeItem {
  code: string;
  name: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  studyProgram: string;
  specialization: string;
}

interface Mentee {
  code: string;
  name: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  studyProgram?: string;
  specialization?: string;
}

const MyMentees: React.FC = () => {
  const navigate = useNavigate();
  const [pendingRequests, setPendingRequests] = useState<MentorRequestExtended[]>([]);
  const [mentees, setMentees] = useState<MenteeItem[]>([]);
  const [selectedMenteeCode, setSelectedMenteeCode] = useState<string | null>(null);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [showTimetableModal, setShowTimetableModal] = useState<boolean>(false);
  const userData = localStorage.getItem('user');
  const user = userData ? JSON.parse(userData) : null;
  const mentorCode = useMemo(() => user?.code, [user]);

  const formatDate = (dateStr: string): string => {
    const d = new Date(dateStr);
    const year = d.getFullYear();
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    const hour = d.getHours().toString().padStart(2, '0');
    const minute = d.getMinutes().toString().padStart(2, '0');
    return `${year}-${month}-${day} ${hour}:${minute}`;
  };

  const fetchPendingRequests = () => {
    if (!mentorCode) return;
    fetch(`http://localhost:5216/api/mentor/requests/${mentorCode}`)
      .then((res) => res.json())
      .then((data: MentorRequestExtended[]) => {
        const pending = data.filter(r => r.status === "Laukiama patvirtinimo");
        setPendingRequests(pending);
        if (pending.length === 0) {
          fetch(`http://localhost:5216/api/mentor/mentees/${mentorCode}`)
            .then(res => res.json())
            .then((menteeData: MenteeItem[]) => setMentees(menteeData))
            .catch(err => console.error(err));
        } else {
          setMentees([]);
        }
      })
      .catch((err) => console.error(err));
  };

  useEffect(() => {
    if (!mentorCode) {
      navigate('/home');
    } else {
      fetchPendingRequests();
      const intervalId = setInterval(fetchPendingRequests, 60000);
      return () => clearInterval(intervalId);
    }
  }, [mentorCode, navigate]);

  // Ensure we fetch mentees regardless of pending requests.
  useEffect(() => {
    if (mentorCode) {
      fetch(`http://localhost:5216/api/mentor/mentees/${mentorCode}`)
        .then(res => res.json())
        .then((data: Mentee[]) => {
          const mappedData = data.map(mentee => ({
            ...mentee,
            studyProgram: mentee.studyProgram || '',
            specialization: mentee.specialization || ''
          }));
          setMentees(mappedData);
        })
        .catch(err => console.error("Error fetching mentees:", err));
    }
  }, [mentorCode]);

  const handleMenteeClick = (code: string) => {
    console.log("Mentee clicked:", code);
    setSelectedMenteeCode(code);
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setSelectedMenteeCode(null);
  };

  const handleReject = async (requestId: number) => {
    let reason = window.prompt("Įveskite atmetimo priežastį (privaloma):");
    if (!reason || reason.trim() === "") {
        alert("Atmetimo priežastis yra privaloma.");
        return;
    }
    try {
        const response = await fetch(`http://localhost:5216/api/mentor/rejectRequest/${requestId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ reason })
        });
        if (response.ok) {
            alert("Prašymas atmestas.");
            fetchPendingRequests(); // Refresh pending requests
        } else {
            const errorMsg = await response.text();
            alert(errorMsg || "Nepavyko atmesti prašymo.");
        }
    } catch (error) {
        console.error("Error rejecting request:", error);
    }
  };

const handleAcceptRequest = async (requestId: number) => {
  try {
    const response = await fetch(`http://localhost:5216/api/mentor/acceptRequest/${requestId}`, {
      method: "POST"
    });
    if (response.ok) {
      alert("Prašymas patvirtintas.");
      fetchPendingRequests(); // Refresh pending requests
    } else {
      const errorMsg = await response.text();
      alert(errorMsg || "Nepavyko patvirtinti prašymo.");
    }
  } catch (error) {
    console.error("Error accepting request:", error);
  }
};

  return (
    <div className="mymentees-container">
      {pendingRequests.length > 0 ? (
        <>
          <h2>Naujų ugdytinių prašymai</h2>
          <table className="mentor-requests-table">
            <thead>
              <tr>
                <th>Ugdytinio kodas</th>
                <th>Vardas</th>
                <th>Pavardė</th>
                <th>El. paštas</th>
                <th>Studijų programa</th>
                <th>Specializacija</th>
                <th>Prašymo data</th>
                <th>Statusas</th>
                <th>Veiksmai</th>
              </tr>
            </thead>
            <tbody>
              {pendingRequests.map((r, index) => (
                <tr key={r.id ? `${r.id}` : `request-${index}`}>
                  <td>{r.menteeCode}</td>
                  <td>{r.name}</td>
                  <td>{r.lastName}</td>
                  <td>{r.email}</td>
                  <td>{r.studyProgram}</td>
                  <td>{r.specialization}</td>
                  <td>{formatDate(r.requestDate)}</td>
                  <td>{r.status}</td>
                  <td>
                    <button className="btn btn-success" onClick={() => { handleAcceptRequest(r.id); }}>
                      Patvirtinti
                    </button>
                    <button className="btn btn-danger" onClick={() => handleReject(r.id)}>
                      Atmesti
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      ) : mentees.length > 0 ? (
        <div className="my-mentees">
          <h2>Mano ugdytiniai</h2>
          <table className="mentees-table">
            <thead>
              <tr>
                <th>Ugdytinio kodas</th>
                <th>Vardas</th>
                <th>Pavardė</th>
                <th>Studijų programa</th>
                <th>Specializacija</th>
                <th>Veiksmai</th>
              </tr>
            </thead>
            <tbody>
              {mentees.map(mentee => (
                <tr key={mentee.code}>
                  <td>{mentee.code}</td>
                  <td>{mentee.name}</td>
                  <td>{mentee.lastName}</td>
                  <td>{mentee.studyProgram || '-'}</td>
                  <td>{mentee.specialization || '-'}</td>
                  <td>
                    <button className='btn btn-primary'
                      onClick={(e) => {
                        e.stopPropagation();
                        // Open timetable modal
                        setShowTimetableModal(true);
                        setSelectedMenteeCode(mentee.code);
                      }}
                    >
                      Tvarkaraštis
                    </button>
                    &nbsp;
                    <button className='btn btn-secondary'
                      onClick={(e) => {
                        e.stopPropagation();
                        // Open semester plan modal
                        setShowModal(true);
                        setSelectedMenteeCode(mentee.code);
                      }}
                    >
                      Semestro planas
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p>Nėra prašymų ir priskirtų ugdytinių.</p>
      )}

      {showModal && selectedMenteeCode && (
        <SemesterPlanModal menteeCode={selectedMenteeCode} onClose={closeModal} />
      )}

      {showTimetableModal && selectedMenteeCode && (
        <MenteeTimetableModal 
          menteeCode={selectedMenteeCode} 
          onClose={() => {
            setShowTimetableModal(false);
            setSelectedMenteeCode(null);
          }}
        />
      )}
    </div>
  );
};

export default MyMentees;