import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import CreateTrainingModal from './CreateTrainingModal';
import CreateAffairModal from './CreateAffairModal';
import EditTrainingModal from './EditTrainingModal';
import EditAffairModal from './EditAffairModal';
import TrainingFeedbackModal from './TrainingFeedbackModal';
import AffairFeedbackModal from './AffairFeedbackModal';
import TrainingFeedbackListModal from './TrainingFeedbackListModal';
import AffairFeedbackListModal from './AffairFeedbackListModal';
import './Events.scss';

interface EventData {
    trainingId?: number;
    eventId: number;
    affairId?: number;
    name: string;
    startTime: string;
    endTime: string;
    address: string;
    comment: string;
    fk_ViceDeadForStudiescode?: string;
}

interface TrainingData {
    trainingId: number;
    eventId: number;
    name: string;
    startTime: string;
    endTime: string;
    address: string;
    comment: string;
  }

  interface AffairData {
    affairId: number;
    eventId: number;
    name: string;
    startTime: string;
    endTime: string;
    address: string;
    comment: string;
  }

const EventsPage: React.FC = () => {
    const [affairs, setAffairs] = useState<EventData[]>([]);
    const [pastAffairs, setPastAffairs] = useState<EventData[]>([]);
    const [trainings, setTrainings] = useState<EventData[]>([]);
    const [pastTrainings, setPastTrainings] = useState<EventData[]>([]);
    const [isAdmin, setIsAdmin] = useState<boolean>(false);
    const [showCreateTrainingModal, setShowCreateTrainingModal] = useState(false);
    const [showCreateAffairModal, setShowCreateAffairModal] = useState(false);
    const [editingTraining, setEditingTraining] = useState<EventData | null>(null);
    const [editingAffair, setEditingAffair] = useState<any | null>(null);
    const [selectedTraining, setSelectedTraining] = useState<EventData | null>(null);
    const [showTrainingDetailModal, setShowTrainingDetailModal] = useState<boolean>(false);
    const [selectedAffair, setSelectedAffair] = useState<EventData | null>(null);
    const [showAffairDetailModal, setShowAffairDetailModal] = useState<boolean>(false);
    const [registeredTrainingEventIds, setRegisteredTrainingEventIds] = useState<number[]>([]);
    const [registeredAffairEventIds, setRegisteredAffairEventIds] = useState<number[]>([]);
    const [isMentor, setIsMentor] = useState<boolean>(false);
    const [awaitingFeedback, setAwaitingFeedback] = useState<TrainingData[]>([]);
    const [feedbackModalTraining, setFeedbackModalTraining] = useState<TrainingData | null>(null);
    const [affairAwaitingFeedback, setAffairAwaitingFeedback] = useState<AffairData[]>([]);
    const [feedbackModalAffair, setFeedbackModalAffair] = useState<AffairData | null>(null);

    const [showTrainingFeedbackListModal, setShowTrainingFeedbackListModal] = useState(false);
    const [selectedTrainingForFeedback, setSelectedTrainingForFeedback] = useState<EventData | null>(null);
    const [trainingFeedbacks, setTrainingFeedbacks] = useState<any[]>([]);

    const [showAffairFeedbackListModal, setShowAffairFeedbackListModal] = useState(false);
    const [selectedAffairForFeedback, setSelectedAffairForFeedback] = useState<EventData | null>(null);
    const [affairFeedbacks, setAffairFeedbacks] = useState<any[]>([]);

    const [page, setPage] = useState<number>(1);
    const pageSize = 2;

    const [affairsPage, setAffairsPage] = useState<number>(1);
    const affairsPageSize = 2;

    const [pastTrainingsPage, setPastTrainingsPage] = useState<number>(1);
    const pastTrainingsPageSize = 2;

    const [pastAffairsPage, setPastAffairsPage] = useState<number>(1);
    const pastAffairsPageSize = 2; 

    const navigate = useNavigate();

    const fetchTrainings = () => {
        fetch('http://localhost:5216/api/events/upcoming/trainings')
            .then(res => res.json())
            .then(data => setTrainings(data))
            .catch(err => console.error('Error fetching trainings:', err));
    };

    const fetchAwaitingFeedback = () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code || user.isAdmin || user.isMentor) return;
        fetch(`http://localhost:5216/api/feedback/training/awaiting/${user.code}`)
            .then(res => res.json())
            .then((data) => {
                setAwaitingFeedback(data);
            })
            .catch(err => console.error("Error fetching awaiting feedback trainings:", err));
    };

    const fetchAffairs = () => {
        fetch('http://localhost:5216/api/events/upcoming/affairs')
          .then(async res => {
             const text = await res.text();
             try {
               const data = JSON.parse(text);
               setAffairs(data);
             } catch(error) {
               console.error("Failed to parse JSON. Response text:", text);
             }
          })
          .catch(err => console.error('Error fetching affairs:', err));
    };

    const fetchAffairAwaitingFeedback = () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code || user.isAdmin || user.isMentor) return;
        fetch(`http://localhost:5216/api/feedback/affair/awaiting/${user.code}`)
            .then(res => res.json())
            .then(data => setAffairAwaitingFeedback(data))
            .catch(err => console.error("Error fetching awaiting feedback affairs:", err));
    };

    const fetchPastTrainings = () => {
        fetch('http://localhost:5216/api/events/past/trainings')
            .then(res => res.json())
            .then(data => setPastTrainings(data))
            .catch(err => console.error('Error fetching past trainings:', err));
    };

    const fetchPastAffairs = () => {
        fetch('http://localhost:5216/api/events/past/affairs')
            .then(res => res.json())
            .then(data => setPastAffairs(data))
            .catch(err => console.error('Error fetching past affairs:', err));
    };

    const fetchRegisteredTrainings = () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code || user.isAdmin || user.isMentor) return;
        fetch(`http://localhost:5216/api/events/registered/trainings?userCode=${user.code}`)
            .then(async res => {
                if (!res.ok) {
                    return [];
                }
                return res.json();
            })
            .then(data => {
                const regIds = data.map((r: any) => Number(r.eventId || r.EventId));
                setRegisteredTrainingEventIds(regIds);
            })
            .catch(err => console.error("Error fetching registered trainings:", err));
    };

    const fetchRegisteredAffairs = () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code || user.isAdmin || user.isMentor) return;
        fetch(`http://localhost:5216/api/events/registered/affairs?userCode=${user.code}`)
            .then(async res => {
                if (!res.ok) {
                    return [];
                }
                return res.json();
            })
            .then(data => {
                const regIds = data.map((r: any) => Number(r.eventId || r.EventId));
                setRegisteredAffairEventIds(regIds);
            })
            .catch(err => console.error("Error fetching registered affairs:", err));
    };

    const handleDeleteTraining = async (trainingId: number) => {
        if (window.confirm("Ar tikrai norite ištrinti šiuos mokymus?")) {
            const response = await fetch(`http://localhost:5216/api/events/trainings/${trainingId}`, {
                method: 'DELETE'
            });
            if (response.ok) {
                fetchTrainings();
                fetchPastTrainings();
            } else {
                console.error('Error deleting training');
            }
        }
    };

    const handleDeleteAffair = async (affairId: number) => {
        if (window.confirm("Ar tikrai norite ištrinti šį renginį?")) {
            const response = await fetch(`http://localhost:5216/api/events/affairs/${affairId}`, {
                method: 'DELETE'
            });
            if (response.ok) {
                fetchAffairs();
                fetchPastAffairs();
            } else {
                console.error('Error deleting affair');
            }
        }
    };

    const handleRegisterTraining = async () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code) {
            alert("User code not found.");
            return;
        }
        if (!selectedTraining) return;
        const payload = {
            eventId: selectedTraining.eventId,
            userCode: user.code
        };
        try {
            const response = await fetch('http://localhost:5216/api/events/register/training', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });
            if (response.ok) {
                alert("Registracija sėkminga!");
                // Re-fetch registered trainings so the button can be disabled appropriately
                fetchRegisteredTrainings();
            } else {
                const errorText = await response.text();
                alert("Klaida: " + errorText);
            }
        } catch(err) {
            console.error("Error registering training:", err);
            alert("Įvyko klaida registruojant mokymus");
        }
    };

    const handleRegisterAffair = async () => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code) {
            alert("User code not found.");
            return;
        }
        if (!selectedAffair) return;
        const payload = {
            eventId: selectedAffair.eventId,
            userCode: user.code
        };
        try {
            const response = await fetch('http://localhost:5216/api/events/register/affair', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });
            if (response.ok) {
                alert("Registracija sėkminga!");
                fetchRegisteredAffairs();
                setShowAffairDetailModal(false);
            } else {
                const errorText = await response.text();
                alert("Klaida: " + errorText);
            }
        } catch(err) {
            console.error("Error registering affair:", err);
            alert("Įvyko klaida registruojant renginį");
        }
    };

    const handleShowTrainingFeedback = (training: EventData) => {
        const trainingId = training.trainingId || (training as any).TrainingId;
        if (!trainingId) {
            console.error("TrainingId not available:", training);
            return;
        }
        setSelectedTrainingForFeedback(training);
        fetch(`http://localhost:5216/api/feedback/training/feedback/${trainingId}`)
            .then(res => res.json())
            .then(data => {
                setTrainingFeedbacks(data);
                setShowTrainingFeedbackListModal(true);
            })
            .catch(err => console.error("Error fetching training feedbacks:", err));
    };
    
    const handleShowAffairFeedback = (affair: EventData) => {
        const affairId = affair.affairId || (affair as any).AffairId;
        if (!affairId) {
            console.error("AffairId not available:", affair);
            return;
        }
        setSelectedAffairForFeedback(affair);
        fetch(`http://localhost:5216/api/feedback/affair/feedback/${affairId}`)
            .then(res => res.json())
            .then(data => {
                setAffairFeedbacks(data);
                setShowAffairFeedbackListModal(true);
            })
            .catch(err => console.error("Error fetching affair feedbacks:", err));
    };

    useEffect(() => {
        fetchAffairs();
        fetchTrainings();
        fetchPastTrainings();
        fetchPastAffairs();
        fetchAffairAwaitingFeedback();
        fetchRegisteredTrainings();
        fetchRegisteredAffairs();
        const storedUser = localStorage.getItem('user');
        if (storedUser) {
            const user = JSON.parse(storedUser);
            setIsMentor(user.isMentor || false); 
            setIsAdmin(user.isAdmin || false); 
            if(!user.isAdmin && !user.isMentor) {
                fetchAwaitingFeedback();
            }
        }
    }, []);

    const openTrainingDetails = (training: EventData) => {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        if (!user.code) {
            setSelectedTraining(training);
            setShowTrainingDetailModal(true);
            return;
        }
        if (user.isAdmin || user.isMentor) {
            setSelectedTraining(training);
            setShowTrainingDetailModal(true);
            return;
        }
        fetch(`http://localhost:5216/api/events/registered/trainings?userCode=${user.code}`)
            .then(async res => {
                if (!res.ok) {
                    return [];
                }
                return res.json();
            })
            .then(data => {
                const regIds = data.map((r: any) => Number(r.eventId || r.EventId));
                setRegisteredTrainingEventIds(regIds);
                setSelectedTraining(training);
                setShowTrainingDetailModal(true);
            })
            .catch(err => {
                console.error("Error fetching registered trainings:", err);
                setSelectedTraining(training);
                setShowTrainingDetailModal(true);
            });
    };

    const openAffairDetails = (affair: EventData) => {
        setSelectedAffair(affair);
        setShowAffairDetailModal(true);
    };

    const displayedTrainings = trainings.slice((page - 1) * pageSize, page * pageSize);
    const totalPages = Math.ceil(trainings.length / pageSize);

    const displayedAffairs = affairs.slice((affairsPage - 1) * affairsPageSize, affairsPage * affairsPageSize);
    const totalAffairPages = Math.ceil(affairs.length / affairsPageSize);   

    const displayedPastTrainings = pastTrainings.slice(
        (pastTrainingsPage - 1) * pastTrainingsPageSize,
        pastTrainingsPage * pastTrainingsPageSize
    );
    const totalPastTrainingPages = Math.ceil(pastTrainings.length / pastTrainingsPageSize);
      
    const displayedPastAffairs = pastAffairs.slice(
    (pastAffairsPage - 1) * pastAffairsPageSize,
    pastAffairsPage * pastAffairsPageSize
    );
    const totalPastAffairPages = Math.ceil(pastAffairs.length / pastAffairsPageSize);

    return (
        <div className="events-page">
            {showCreateTrainingModal && (
                <CreateTrainingModal 
                    onClose={() => setShowCreateTrainingModal(false)} 
                    onTrainingCreated={() => {
                        fetchTrainings();
                        setShowCreateTrainingModal(false);
                    }}
                />
            )}
            {editingTraining && (
                <EditTrainingModal
                    training={editingTraining as Required<Pick<EventData, 'trainingId' | 'name' | 'startTime' | 'endTime' | 'address' | 'comment'>>}
                    onClose={() => setEditingTraining(null)}
                    onTrainingUpdated={() => {
                        fetchTrainings();
                        setEditingTraining(null);
                    }}
                />
            )}
            {showCreateAffairModal && (
                <CreateAffairModal
                    onClose={() => setShowCreateAffairModal(false)}
                    onAffairCreated={() => {
                        fetchAffairs();
                        setShowCreateAffairModal(false);
                    }}
                />
            )}
            {editingAffair && (
                <EditAffairModal
                    affair={editingAffair}
                    onClose={() => setEditingAffair(null)}
                    onAffairUpdated={() => {
                        fetchAffairs();
                        setEditingAffair(null);
                    }}
                />
            )}
            <div className="events-container">
                <div className="column trainings-column">
                    { showTrainingDetailModal && selectedTraining && (
                        <div style={{
                            position: 'fixed',
                            top: 0, left: 0, right: 0, bottom: 0,
                            backgroundColor: 'rgba(0,0,0,0.5)',
                            display: 'flex',
                            justifyContent: 'center',
                            alignItems: 'center',
                            zIndex: 1000
                        }}>
                            <div style={{
                                backgroundColor: '#fff',
                                padding: '20px',
                                borderRadius: '8px',
                                maxWidth: '600px',
                                width: '90%'
                            }}>
                                <h3 style={{ textAlign: 'center' }}>{selectedTraining.name}</h3>
                                <p>
                                    Labas, {JSON.parse(localStorage.getItem('user') || '{}').name || 'svečias'}! <br />
                                    Maloniai kviečiame Tave sudalyvauti mokymuose "{selectedTraining.name}". <br />
                                    Šie mokymai vyks nuo {new Date(selectedTraining.startTime).toLocaleString('lt-LT')}
                                    iki {new Date(selectedTraining.endTime).toLocaleString('lt-LT')} adresu {selectedTraining.address}. <br />
                                    Nepraleisk progos sužinoti daugiau!
                                </p>
                                <p style={{ fontSize: '0.9em', color: '#666' }}>
                                    {selectedTraining.comment && `Komentaras: ${selectedTraining.comment}`}
                                </p>

                                <button 
                                    style={{ float: 'right' }} 
                                    className="btn btn-primary" 
                                    onClick={() => setShowTrainingDetailModal(false)}
                                >
                                    Uždaryti
                                </button>
                                { (!isAdmin && !isMentor) && (
                                    <button 
                                        className="btn btn-primary" 
                                        onClick={handleRegisterTraining}
                                        disabled={registeredTrainingEventIds.includes(selectedTraining.eventId)}
                                        style={registeredTrainingEventIds.includes(selectedTraining.eventId)
                                            ? { opacity: 0.5, cursor: 'not-allowed', float: 'left' }
                                            : {float: 'left' }}
                                    >
                                        Registruotis į mokymus
                                    </button>
                                )}
                            </div>
                        </div>
                    )}

                    <h2>Artėjantys mokymai</h2>
                    {isAdmin && (
                        <button className="btn btn-primary" onClick={() => setShowCreateTrainingModal(true)}>
                            Sukurti mokymus
                        </button>
                    )}
                    <br></br><br></br><br></br>
                    {trainings.length === 0 ? (
                        <p>Nėra artėjančių mokymų</p>
                    ) : (
                        <>
                        <div className="cards-container">
                              {displayedTrainings.map((e, index) => (
                                  <div className="card" key={`training-${e.trainingId !== undefined ? e.trainingId : `fallback-${index}`}`}>
                                      <h3>{e.name}</h3>
                                      <p>
                                        {new Date(e.startTime).toLocaleDateString('lt-LT')}{" "}
                                        {new Date(e.startTime).toLocaleTimeString('lt-LT')}–  
                                        {new Date(e.endTime).toLocaleDateString('lt-LT')}{" "}
                                        {new Date(e.endTime).toLocaleTimeString('lt-LT')}
                                      </p>
                                      <p>Adresas: {e.address}</p>
                                      { isAdmin ? (
                                          <div className="card-actions">
                                              <button style={{ marginLeft: '-10px' }} className="btn btn-primary-outline" onClick={() => setEditingTraining(e)}>
                                                  Redaguoti
                                              </button>
                                              <button className="btn btn-primary" onClick={() => openTrainingDetails(e)} style={{ height: '50px' }}>
                                                  Peržiūrėti kvietimą
                                              </button>
                                              <button className="btn btn-danger" onClick={() => handleDeleteTraining(e.trainingId!)}>
                                                  Ištrinti
                                              </button>
                                          </div>
                                      ) : (
                                          <div className="card-actions">
                                              <button className="btn btn-primary" onClick={() => openTrainingDetails(e)} style={{ marginLeft: '20%' }}>
                                                  Peržiūrėti kvietimą
                                              </button>
                                          </div>
                                      )}
                                  </div>
                              ))}
                          </div>
                          <div style={{ marginTop: '1rem', textAlign: 'center' }}>
                          <button 
                              className="btn btn-primary-outline" 
                              onClick={() => setPage(page + 1)} 
                              disabled={page === totalPages}
                          >
                              Kiti
                          </button>
                          <span style={{ margin: '0 1rem' }}>{page} / {totalPages}</span>
                          <button 
                              className="btn btn-primary-outline" 
                              onClick={() => setPage(page - 1)} 
                              disabled={page === 1}
                          >
                              Ankstesni
                          </button>
                      </div>
                    </>
                    )}

                    {isAdmin && (
                        <>
                            <div className="past-header">
                                <h2>Praėję mokymai</h2>
                            </div>
                            {pastTrainings.length === 0 ? (
                                <p>Nėra praėjusių mokymų</p>
                            ) : (
                                <>
                                    <div className="cards-container">
                                    {displayedPastTrainings.map((e, index) => (
                                        <div className="card" key={`past-training-${e.trainingId !== undefined ? e.trainingId : `fallback-${index}`}`}>
                                        <h3>{e.name}</h3>
                                        <p>
                                            {new Date(e.startTime).toLocaleDateString('lt-LT')}{" "}
                                            {new Date(e.startTime).toLocaleTimeString('lt-LT')}– 
                                            {new Date(e.endTime).toLocaleDateString('lt-LT')}{" "}
                                            {new Date(e.endTime).toLocaleTimeString('lt-LT')}
                                        </p>
                                        <p>Adresas: {e.address}</p>
                                        {isAdmin && (
                                            <div className="card-actions">
                                            <button style={{ marginLeft: '-10px' }} className="btn btn-primary-outline" onClick={() => setEditingTraining(e)}>
                                                Redaguoti
                                            </button>
                                            <button className="btn btn-primary" onClick={() => handleShowTrainingFeedback(e)}>
                                                Atsiliepimai
                                            </button>
                                            <button className="btn btn-danger" onClick={() => handleDeleteTraining(e.trainingId!)}>
                                                Ištrinti
                                            </button>
                                            </div>
                                        )}
                                        </div>
                                    ))}
                                    </div>
                                    <div style={{ marginTop: '1rem', textAlign: 'center' }}>
                                        <button 
                                            className="btn btn-primary-outline" 
                                            onClick={() => setPastTrainingsPage(pastTrainingsPage - 1)} 
                                            disabled={pastTrainingsPage === 1}
                                        >
                                            Ankstesni
                                        </button>
                                        <span style={{ margin: '0 1rem' }}>{pastTrainingsPage} / {totalPastTrainingPages}</span>
                                        <button 
                                            className="btn btn-primary-outline" 
                                            onClick={() => setPastTrainingsPage(pastTrainingsPage + 1)} 
                                            disabled={pastTrainingsPage === totalPastTrainingPages}
                                        >
                                            Kiti
                                        </button>
                                    </div>
                                </>
                            )}
                        </>
                    )}

                    {(!isAdmin && !isMentor) && (
                        <>
                            <h2 style={{ marginTop: '40px' }}>Mokymai, laukiantys grįžtamojo ryšio</h2>
                            {awaitingFeedback.length === 0 ? (
                                <p>Nėra mokymų, laukiančių grįžtamojo ryšio</p>
                            ) : (
                                <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '10px' }}>
                                    <thead>
                                        <tr>
                                            <th style={{ border: '1px solid #ddd', padding: '8px' }}>Pavadinimas</th>
                                            <th style={{ border: '1px solid #ddd', padding: '8px' }}>Data</th>
                                            <th style={{ border: '1px solid #ddd', padding: '8px' }}>Veiksmai</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {awaitingFeedback.map((t, idx) => (
                                            <tr key={idx}>
                                                <td style={{ border: '1px solid #ddd', padding: '8px' }}>{t.name}</td>
                                                <td style={{ border: '1px solid #ddd', padding: '8px' }}>
                                                    {new Date(t.startTime).toLocaleString('lt-LT')}-
                                                    {new Date(t.endTime).toLocaleString('lt-LT')}
                                                </td>
                                                <td style={{ border: '1px solid #ddd', padding: '8px' }}>
                                                    <button 
                                                        className="btn btn-primary" 
                                                        onClick={() => setFeedbackModalTraining(t)}
                                                    >
                                                        Palikti atsiliepimą
                                                    </button>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            )}
                        </>
                    )}

                    {feedbackModalTraining && (
                        <TrainingFeedbackModal 
                            training={feedbackModalTraining} 
                            onClose={() => setFeedbackModalTraining(null)}
                            onFeedbackSubmitted={fetchAwaitingFeedback}
                        />
                    )}
            </div>
                <div className="column affairs-column">
                    <h2>Artėjantys renginiai</h2>
                    {isAdmin && (
                        <button className="btn btn-primary" onClick={() => setShowCreateAffairModal(true)}>
                            Sukurti renginį
                        </button>
                    )}
                    <br></br><br></br><br></br>
                    {affairs.length === 0 ? (
                        <p>Nėra artėjančių renginių</p>
                    ) : (
                        <>
                            <div className="cards-container">
                                {displayedAffairs.map((e, index) => (
                                    <div className="card" key={`affair-${e.eventId !== undefined ? e.eventId : `fallback-${index}`}`}>
                                        <h3>{e.name}</h3>
                                        <p>
                                            {new Date(e.startTime).toLocaleDateString('lt-LT')}{" "}
                                            {new Date(e.startTime).toLocaleTimeString('lt-LT')}– 
                                            {new Date(e.endTime).toLocaleDateString('lt-LT')}{" "}
                                            {new Date(e.endTime).toLocaleTimeString('lt-LT')}
                                        </p>
                                        <p>Adresas: {e.address}</p>
                                        {isAdmin ? (
                                            <div className="card-actions">
                                                <button style={{ marginLeft: '-10px' }} className="btn btn-primary-outline" onClick={() => setEditingAffair(e)}>
                                                    Redaguoti
                                                </button>
                                                <button 
                                                    className="btn btn-primary" 
                                                    onClick={() => openAffairDetails(e)}
                                                    style={{ height: '50px' }}
                                                >
                                                    Peržiūrėti kvietimą
                                                </button>
                                                <button className="btn btn-danger" onClick={() => e.affairId !== undefined && handleDeleteAffair(e.affairId)}>
                                                    Ištrinti
                                                </button>
                                            </div>
                                        ) : (
                                            <div className="card-actions">
                                                <button className="btn btn-primary" onClick={() => openAffairDetails(e)} style={{ marginLeft: '20%' }}>
                                                    Peržiūrėti kvietimą
                                                </button>
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                            <div style={{ marginTop: '1rem', textAlign: 'center' }}>
                                <button 
                                    className="btn btn-primary-outline" 
                                    onClick={() => setAffairsPage(affairsPage - 1)} 
                                    disabled={affairsPage === 1}
                                >
                                    Ankstesni
                                </button>
                                <span style={{ margin: '0 1rem' }}>{affairsPage} / {totalAffairPages}</span>
                                <button 
                                    className="btn btn-primary-outline" 
                                    onClick={() => setAffairsPage(affairsPage + 1)} 
                                    disabled={affairsPage === totalAffairPages}
                                >
                                    Kiti
                                </button>
                            </div>
                        </>
                    )}

                    {isAdmin && (
                    <>
                        <div className="past-header">
                        <h2>Praėję renginiai</h2>
                        <br />
                        </div>
                        {pastAffairs.length === 0 ? (
                        <p>Nėra praėjusių renginių</p>
                        ) : (
                        <>
                            <div className="cards-container">
                            {displayedPastAffairs.map((e, index) => (
                                <div className="card" key={`past-affair-${e.affairId !== undefined ? e.affairId : `fallback-${index}`}`}>
                                <h3>{e.name}</h3>
                                <p>
                                    {new Date(e.startTime).toLocaleDateString('lt-LT')}{" "}
                                    {new Date(e.startTime).toLocaleTimeString('lt-LT')}– 
                                    {new Date(e.endTime).toLocaleDateString('lt-LT')}{" "}
                                    {new Date(e.endTime).toLocaleTimeString('lt-LT')}
                                </p>
                                <p>Adresas: {e.address}</p>
                                {isAdmin && (
                                    <div className="card-actions">
                                    <button style={{ marginLeft: '-10px' }} className="btn btn-primary-outline" onClick={() => setEditingAffair(e)}>
                                        Redaguoti
                                    </button>
                                    <button className="btn btn-primary" onClick={() => handleShowAffairFeedback(e)}>
                                        Atsiliepimai
                                    </button>
                                    <button className="btn btn-danger" onClick={() => handleDeleteAffair(e.affairId!)}>
                                        Ištrinti
                                    </button>
                                    </div>
                                )}
                                </div>
                            ))}
                            </div>
                            <div style={{ marginTop: '1rem', textAlign: 'center' }}>
                            <button 
                                className="btn btn-primary-outline" 
                                onClick={() => setPastAffairsPage(pastAffairsPage - 1)} 
                                disabled={pastAffairsPage === 1}
                            >
                                Ankstesni
                            </button>
                            <span style={{ margin: '0 1rem' }}>{pastAffairsPage} / {totalPastAffairPages}</span>
                            <button 
                                className="btn btn-primary-outline" 
                                onClick={() => setPastAffairsPage(pastAffairsPage + 1)} 
                                disabled={pastAffairsPage === totalPastAffairPages}
                            >
                                Kiti
                            </button>
                            </div>
                        </>
                        )}
                    </>
                    )}

                    {(!isAdmin && !isMentor) && (
                        <>
                            <h2 style={{ marginTop: '40px' }}>Renginiai, laukiantys grįžtamojo ryšio</h2>
                            {affairAwaitingFeedback.length === 0 ? (
                                <p>Nėra renginių, laukiančių grįžtamojo ryšio</p>
                            ) : (
                                <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '10px' }}>
                                    <thead>
                                        <tr>
                                            <th style={{ border: '1px solid #ddd', padding: '8px' }}>Pavadinimas</th>
                                            <th style={{ border: '1px solid #ddd', padding: '8px' }}>Data</th>
                                            <th style={{ border: '1px solid #ddd', padding: '8px' }}>Veiksmai</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {affairAwaitingFeedback.map((a, idx) => (
                                            <tr key={idx}>
                                                <td style={{ border: '1px solid #ddd', padding: '8px' }}>{a.name}</td>
                                                <td style={{ border: '1px solid #ddd', padding: '8px' }}>
                                                    {new Date(a.startTime).toLocaleString('lt-LT')}-
                                                    {new Date(a.endTime).toLocaleString('lt-LT')}
                                                </td>
                                                <td style={{ border: '1px solid #ddd', padding: '8px' }}>
                                                    <button 
                                                        className="btn btn-primary" 
                                                        onClick={() => setFeedbackModalAffair(a)}
                                                    >
                                                        Palikti atsiliepimą
                                                    </button>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            )}
                        </>
                    )}

                    {feedbackModalAffair && (
                        <AffairFeedbackModal 
                            affair={feedbackModalAffair} 
                            onClose={() => setFeedbackModalAffair(null)}
                            onFeedbackSubmitted={fetchAffairAwaitingFeedback}
                        />
                    )}
                </div>
            </div>

            

            {showAffairDetailModal && selectedAffair && (
                <div style={{
                    position: 'fixed',
                    top: 0, left: 0, right: 0, bottom: 0,
                    backgroundColor: 'rgba(0,0,0,0.5)',
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    zIndex: 1000
                }}>
                    <div style={{
                        backgroundColor: '#fff',
                        padding: '20px',
                        borderRadius: '8px',
                        maxWidth: '600px',
                        width: '90%'
                    }}>
                        <h3 style={{textAlign: 'center'}}>{selectedAffair.name}</h3>
                        <p>
                          Labas, {JSON.parse(localStorage.getItem('user') || '{}').name || 'svečias'}! <br />
                          {new Date(selectedAffair.startTime).toLocaleString('lt-LT')}–{new Date(selectedAffair.endTime).toLocaleString('lt-LT')} adresu {selectedAffair.address} yra organizuojamas renginys: "{selectedAffair.name}". <br />
                          Maloniai kvičiame tapti šio renginio dalimi!
                        </p>
                        {selectedAffair.comment && (
                            <p style={{fontSize: '0.9em', color: '#666'}}>
                                Komentaras: {selectedAffair.comment}
                            </p>
                        )}
                        {/* Only mentees (not admins and not mentors) can register */}
                        { (!isAdmin && !isMentor) && (
                            <button 
                                className="btn btn-primary" 
                                onClick={handleRegisterAffair}
                                disabled={registeredTrainingEventIds.includes(selectedAffair.eventId)}
                                style={registeredTrainingEventIds.includes(selectedAffair.eventId)
                                        ? { opacity: 0.5, cursor: 'not-allowed' }
                                        : {}}
                            >
                                Registruotis į renginį
                            </button>
                        )}
                        <button 
                            style={{ float:'right' }} 
                            className="btn btn-primary" 
                            onClick={() => setShowAffairDetailModal(false)}
                        >
                            Uždaryti
                        </button>
                    </div>
                </div>
            )}

            {showTrainingFeedbackListModal && selectedTrainingForFeedback && (
                <TrainingFeedbackListModal
                    trainingName={selectedTrainingForFeedback.name}
                    feedbacks={trainingFeedbacks}
                    onClose={() => {
                        setShowTrainingFeedbackListModal(false);
                        setTrainingFeedbacks([]);
                    }}
                />
            )}

            {showAffairFeedbackListModal && selectedAffairForFeedback && (
                <AffairFeedbackListModal
                    affairName={selectedAffairForFeedback.name}
                    feedbacks={affairFeedbacks}
                    onClose={() => {
                        setShowAffairFeedbackListModal(false);
                        setAffairFeedbacks([]);
                    }}
                />
            )}
        </div>
    );
};

export default EventsPage;