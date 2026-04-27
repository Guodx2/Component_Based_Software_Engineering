import './Accounts.scss';
import { useState, useEffect } from 'react';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend } from 'chart.js';
import { Bar, Pie } from 'react-chartjs-2';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend);

function Accounts() {
    // Fetch study programs, specializations, and now departments from database.
    const [studyPrograms, setStudyPrograms] = useState<{value: string, label: string}[]>([]);
    const [specializations, setSpecializations] = useState<{value: string, label: string}[]>([]);
    const [departments, setDepartments] = useState<{value: string, label: string}[]>([]);
    const [mentorPage, setMentorPage] = useState<number>(1);
    const [menteePage, setMenteePage] = useState<number>(1);

    const renderMentorPagination = () => (
        <div style={{ marginTop: '10px', textAlign: 'center' }}>
          <button 
            disabled={mentorPage === 1}
            onClick={() => setMentorPage(mentorPage - 1)}
          >
            Ankstesnis
          </button>
          <span style={{ margin: '0 10px' }}>
            Puslapis {mentorPage} iš {totalMentorPages}
          </span>
          <button 
            disabled={mentorPage === totalMentorPages}
            onClick={() => setMentorPage(mentorPage + 1)}
          >
            Kitas
          </button>
        </div>
      );
        
    const renderMenteePagination = () => (
    <div style={{ marginTop: '10px', textAlign: 'center' }}>
        <button 
        disabled={menteePage === 1}
        onClick={() => setMenteePage(menteePage - 1)}
        >
        Ankstesnis
        </button>
        <span style={{ margin: '0 10px' }}>
        Puslapis {menteePage} iš {totalMenteePages}
        </span>
        <button 
        disabled={menteePage === totalMenteePages}
        onClick={() => setMenteePage(menteePage + 1)}
        >
        Kitas
        </button>
    </div>
    );

    // Other states remain unchanged.
    const [mentors, setMentors] = useState<{
        id: string,
        name: string,
        lastName: string,
        email: string,
        phone: string,
        department: number
    }[]>([]);

    const [mentorSearchTerm, setMentorSearchTerm] = useState<string>('');
    const [menteeSearchTerm, setMenteeSearchTerm] = useState<string>('');
    const [submittedSearchTerm, setSubmittedSearchTerm] = useState<string>('');
    const [otherSearchTerm, setOtherSearchTerm] = useState<string>('');
    
    const filteredMentors = mentors.filter(m =>
        m.name.toLowerCase().includes(mentorSearchTerm.toLowerCase()) ||
        m.lastName.toLowerCase().includes(mentorSearchTerm.toLowerCase()) ||
        m.email.toLowerCase().includes(mentorSearchTerm.toLowerCase())
    );

    const mentorsPerPage = 20;
    const totalMentorPages = Math.ceil(filteredMentors.length / mentorsPerPage);
    const mentorStartIndex = (mentorPage - 1) * mentorsPerPage;
    const mentorDisplayed = filteredMentors.slice(mentorStartIndex, mentorStartIndex + mentorsPerPage);
    
    const [mentees, setMentees] = useState<{
        id: string,
        name: string,
        lastName: string,
        email: string,
        phone: string,
        studyProgram: number,
        specialization?: number | null
    }[]>([]);

    const filteredMentees = mentees.filter(m =>
        m.name.toLowerCase().includes(menteeSearchTerm.toLowerCase()) ||
        m.lastName.toLowerCase().includes(menteeSearchTerm.toLowerCase()) ||
        m.email.toLowerCase().includes(menteeSearchTerm.toLowerCase())
    );

    /*const filteredSubmitted = submittedUsers.filter(u =>
        u.name.toLowerCase().includes(submittedSearchTerm.toLowerCase()) ||
        u.lastName.toLowerCase().includes(submittedSearchTerm.toLowerCase()) ||
        u.email.toLowerCase().includes(submittedSearchTerm.toLowerCase())
    );

    const filteredOthers = otherAccounts.filter(u =>
        u.name.toLowerCase().includes(otherSearchTerm.toLowerCase()) ||
        u.lastName.toLowerCase().includes(otherSearchTerm.toLowerCase()) ||
        u.email.toLowerCase().includes(otherSearchTerm.toLowerCase())
    );*/

    const menteesPerPage = 20;
    const totalMenteePages = Math.ceil(filteredMentees.length / menteesPerPage);
    const menteeStartIndex = (menteePage - 1) * menteesPerPage;
    const menteeDisplayed = filteredMentees.slice(menteeStartIndex, menteeStartIndex + menteesPerPage);
    
    const [submittedUsers, setSubmittedUsers] = useState<{ 
        id: string,
        name: string,
        email: string,
        lastName: string,
        phone: string,
        studyProgram: string,
        department: string,
        specialization?: string
    }[]>([]);
    
    const [otherAccounts, setOtherAccounts] = useState<{
        id: string,
        name: string,
        lastName: string,
        email: string,
        phone: string
    }[]>([]);

    // New state for rejection history modal
    const [history, setHistory] = useState<Array<{ rejectedAt: string, reason: string }>>([]);
    const [modalOpen, setModalOpen] = useState(false);

    // New state for selected mentor’s mentees
    const [selectedMentorMentees, setSelectedMentorMentees] = useState<any[]>([]);
    const [mentorModalOpen, setMentorModalOpen] = useState(false);

    // Reusable function for fetching all data
    const refreshData = () => {
        // Fetch mentors
        fetch('http://localhost:5216/api/user/mentors')
            .then(response => response.text())
            .then(text => {
                try {
                    const data = JSON.parse(text);
                    setMentors(data);
                } catch (error) {
                    console.error('Error parsing mentors JSON:', error);
                }
            });
            
        // Fetch mentees
        fetch('http://localhost:5216/api/user/mentees')
            .then(response => response.text())
            .then(text => {
                try {
                    const data = JSON.parse(text);
                    setMentees(data);
                } catch (error) {
                    console.error('Error parsing mentees JSON:', error);
                }
            });
    
        // Fetch submitted users
        fetch('http://localhost:5216/api/user/submitted')
            .then(response => response.text())
            .then(text => {
                try {
                    const data = JSON.parse(text);
                    setSubmittedUsers(data);
                } catch (error) {
                    console.error('Failed to parse submitted users JSON:', error);
                }
            });
    
        // Fetch other accounts
        fetch('http://localhost:5216/api/user/others')
            .then(response => response.text())
            .then(text => {
                try {
                    const data = JSON.parse(text);
                    setOtherAccounts(data);
                } catch (error) {
                    console.error('Failed to parse other accounts JSON:', error);
                }
            });
    };
    
    // New effect for fetching departments.
    useEffect(() => {
        fetch('http://localhost:5216/api/departments')
            .then(res => res.json())
            .then((data) => {
                setDepartments(data);
            })
            .catch(err => console.error("Error fetching departments:", err));
    }, []);

    // Existing effect for fetching study programs and specializations remains unchanged.
    useEffect(() => {
        fetch('http://localhost:5216/api/studyprograms')
            .then(res => res.json())
            .then((data) => {
                setStudyPrograms(data);
            })
            .catch(err => console.error("Error fetching study programs:", err));

        fetch('http://localhost:5216/api/specializations')
            .then(res => res.json())
            .then((data) => {
                setSpecializations(data);
            })
            .catch(err => console.error("Error fetching specializations:", err));
    }, []);

    useEffect(() => {
        refreshData();
    }, []);
    
    const handleDelete = (row: { name: string, email: string, id: string }) => {
        if (window.confirm(`Ar tikrai norite pašalinti ${row.name}?`)) {
            fetch(`http://localhost:5216/api/user/delete/${row.id}`, {
                method: 'DELETE'
            }).then(response => {
                if (response.ok) {
                    refreshData(); // Refresh UI if deletion is successful
                } else {
                    response.text().then(errorMessage => {
                        alert(errorMessage || "Delete failed.");
                    });
                }
            }).catch(err => {
                console.error('Delete failed.', err);
            });
        }
    };

    const handleAccept = (userId: string) => {
        fetch(`http://localhost:5216/api/user/accept/${userId}`, {
            method: 'POST'
        }).then(response => {
            if (response.ok) {
                refreshData();
            }
        });
    };

    const handleRejectWithReason = (userId: string) => {
        const reason = prompt("Įveskite atmetimo priežastį (privaloma):");
        if (reason) {
            fetch(`http://localhost:5216/api/user/reject/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ reason })
            }).then(response => {
                if (response.ok) {
                    refreshData();
                }
            });
        }
    };

    const viewRejectionHistory = (userId: string) => {
        fetch(`http://localhost:5216/api/user/rejection-history/${userId}`)
            .then(response => response.json())
            .then(data => {
                if (data && data.length > 0) {
                    setHistory(data);
                    setModalOpen(true);
                } else {
                    alert("Šiam naudotojui nėra atmetimo istorijos.");
                }
            })
            .catch(error => {
                console.error("Error fetching rejection history:", error);
            });
    };

    const viewMentorMentees = (mentorId: string) => {
        fetch(`http://localhost:5216/api/user/mentees/mentor/${mentorId}`)
            .then(res => res.json())
            .then((data: any[]) => {
                setSelectedMentorMentees(data);
                setMentorModalOpen(true);
            })
            .catch(err => {
                console.error("Error fetching mentor mentees:", err);
            });
    };

    const [educationalResults, setEducationalResults] = useState<any>(null);
    const [eduModalOpen, setEduModalOpen] = useState<boolean>(false);
    const viewEducationalResults = (menteeId: string) => {
        fetch(`http://localhost:5216/api/mentee/educationalresults/${menteeId}`)
            .then(res => res.json())
            .then((data) => {
                console.log("Educational results", data);
                setEducationalResults(data);
                setEduModalOpen(true);
            })
            .catch(err => {
                console.error("Error fetching educational results:", err);
            });
    };

    const chartData = educationalResults && {
        labels: educationalResults.results.map((res: any) => res.taskName),
        datasets: [
            {
                label: 'Įvertinimas',
                data: educationalResults.results.map((res: any) => res.feedbackRating ?? 0),
                backgroundColor: 'rgba(75, 192, 192, 0.6)',
            },
        ],
    };

    const chartOptions = {
        scales: {
            y: {
                beginAtZero: true,
                max: 5,
            },
        },
        plugins: {
            legend: {
                display: false,
            },
            title: {
                display: true,
                text: 'Užduočių įvertinimai',
            },
        },
    };

    const [activityStats, setActivityStats] = useState<{
        totalTrainings: number,
        registeredTrainings: number,
        totalAffairs: number,
        registeredAffairs: number
      } | null>(null);

    useEffect(() => {
        const menteeCodeForStats = educationalResults?.semesterPlan?.menteeCode;
        if (menteeCodeForStats) {
            fetch(`http://localhost:5216/api/user/activityStats/${menteeCodeForStats}`)
                .then(res => res.json())
                .then(data => setActivityStats(data))
                .catch(err => console.error("Error fetching activity stats:", err));
        }
    }, [educationalResults]);

    const trainingPieData = activityStats ? {
        labels: ['Mokymai, į kuriuos registravosi', 'Mokymai, į kuriuose nesiregistravo'],
        datasets: [{
            data: [
              activityStats.registeredTrainings, 
              activityStats.totalTrainings - activityStats.registeredTrainings
            ],
            backgroundColor: ['#36A2EB', '#FF6384'],
        }]
    } : undefined;
  
    const affairPieData = activityStats ? {
        labels: ['Renginiai, į kuriuos registravosi', 'Renginiai, į kuriuos nesiregistravo'],
        datasets: [{
            data: [
              activityStats.registeredAffairs, 
              activityStats.totalAffairs - activityStats.registeredAffairs
            ],
            backgroundColor: ['#4BC0C0', '#FFCE56'],
        }]
    } : undefined;

    const formatRejectionDate = (dateString: string): string => {
        const d = new Date(dateString);
        const year = d.getFullYear();
        const month = String(d.getMonth() + 1).padStart(2, "0");
        const day = String(d.getDate()).padStart(2, "0");
        const hours = String(d.getHours()).padStart(2, "0");
        const minutes = String(d.getMinutes()).padStart(2, "0");
        return `${year}-${month}-${day} ${hours}:${minutes}`;
    };

    return (
        <div className="accounts-page">
            {/* Mentors Section */}
            <div className="table-container">
                <h3>Mentorių paskyros</h3>
                {mentors.length > 0 ? (
                    <>
                    <div style={{ textAlign: 'center', marginBottom: '10px' }}>
                        <input
                            type="text"
                            placeholder="Ieškoti mentorių..."
                            value={mentorSearchTerm}
                            onChange={(e) => { 
                                setMentorSearchTerm(e.target.value);
                                setMentorPage(1); 
                            }}
                            style={{ padding: '8px', width: '80%', maxWidth: '400px', borderRadius: '4px', border: '1px solid #ccc' }}
                        />
                    </div>
                        <table>
                            <thead className="table-header">
                                <tr>
                                    <th>Vid. kodas</th>
                                    <th>Vardas</th>
                                    <th>Pavardė</th>
                                    <th>El. paštas</th>
                                    <th>Tel. nr.</th>
                                    <th>Katedra</th>
                                    <th>Veiksmai</th>
                                </tr>
                            </thead>
                            <tbody>
                                {mentorDisplayed.map(user => {
                                    const departmentLabel = user.department
                                        ? (departments.find(dep => dep.value === user.department.toString())?.label || '-')
                                        : '-';
                                    return (
                                        <tr key={user.id}>
                                            <td>{user.id}</td>
                                            <td>{user.name}</td>
                                            <td>{user.lastName}</td>
                                            <td>{user.email}</td>
                                            <td>{user.phone}</td>
                                            <td>{departmentLabel}</td>
                                            <td>
                                                <button className="btn btn-outline-info" onClick={() => viewMentorMentees(user.id)}>
                                                    Peržiūrėti ugdytinius
                                                </button>
                                                <button className="btn btn-outline-primary" onClick={() => viewRejectionHistory(user.id)}>Istorija</button>
                                                <button className="btn-delete" onClick={() => handleDelete({ name: user.name, email: user.email, id: user.id})}>Pašalinti</button>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                        {totalMentorPages > 1 && renderMentorPagination()}
                    </>
                ) : (
                    <p>Nėra mentorių paskyrų.</p>
                )}
            </div>

            {/* Mentor Mentees Modal */}
            {mentorModalOpen && (
                <div style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
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
                        <h3>Mentoriaus ugdytiniai</h3>
                        {selectedMentorMentees.length > 0 ? (
                            <table>
                                <thead>
                                    <tr>
                                        <th>Vid. kodas</th>
                                        <th>Vardas</th>
                                        <th>Pavardė</th>
                                        <th>El. paštas</th>
                                        <th>Tel. nr.</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {selectedMentorMentees.map(mentee => (
                                        <tr key={mentee.id}>
                                            <td>{mentee.id}</td>
                                            <td>{mentee.name}</td>
                                            <td>{mentee.lastName}</td>
                                            <td>{mentee.email}</td>
                                            <td>{mentee.phone}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        ) : (
                            <p>Šiam mentoriui nėra priskirtų ugdytinių.</p>
                        )}
                        <button style={{float:'right'}} className='btn-delete' onClick={() => setMentorModalOpen(false)}>
                            Uždaryti
                        </button>
                    </div>
                </div>
            )}

            {/* Mentees Section */}
            <div className="table-container">
                <h3>Ugdytinių paskyros</h3>
                {mentees.length > 0 ? (
                    <>
                    <div style={{ textAlign: 'center', marginBottom: '10px' }}>
                        <input
                            type="text"
                            placeholder="Ieškoti ugdytinių..."
                            value={menteeSearchTerm}
                            onChange={(e) => {
                                setMenteeSearchTerm(e.target.value);
                                setMenteePage(1);
                            }}
                            style={{ padding: '8px', width: '80%', maxWidth: '400px', borderRadius: '4px', border: '1px solid #ccc' }}
                        />
                    </div>
                        <table>
                            <thead>
                                <tr>
                                    <th>Vid. kodas</th>
                                    <th>Vardas</th>
                                    <th>Pavardė</th>
                                    <th>El. paštas</th>
                                    <th>Tel. nr.</th>
                                    <th>Studijų programa</th>
                                    <th>Specializacija</th>
                                    <th>Veiksmai</th>
                                </tr>
                            </thead>
                            <tbody>
                                {menteeDisplayed.map(user => {
                                    const studyProgramLabel = user.studyProgram 
                                        ? (studyPrograms.find(sp => sp.value === user.studyProgram.toString())?.label || '-') 
                                        : '-';
                                    const specializationLabel = (user.specialization != null)
                                        ? (specializations.find(s => s.value === user.specialization!.toString())?.label || '-')
                                        : '-';
                                    return (
                                        <tr key={user.id}>
                                            <td>{user.id}</td>
                                            <td>{user.name}</td>
                                            <td>{user.lastName}</td>
                                            <td>{user.email}</td>
                                            <td>{user.phone}</td>
                                            <td>{studyProgramLabel}</td>
                                            <td>{specializationLabel}</td>
                                            <td>
                                                <button className="btn btn-outline-primary" onClick={() => viewEducationalResults(user.id)}>Ugdymo rezultatai</button>
                                                <button className="btn btn-outline-primary" onClick={() => viewRejectionHistory(user.id)}>Istorija</button>
                                                <button className="btn-delete" onClick={() => handleDelete({ name: user.name, email: user.email, id: user.id})}>Pašalinti</button>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                        {totalMenteePages > 1 && renderMenteePagination()}
                    </>
                ) : (
                    <p>Nėra ugdytinių paskyrų.</p>
                )}
            </div>

            {/* Submitted Users Section */}
            <div className="table-container">
                <h3>Laukia patvirtinimo</h3>
                {submittedUsers.length > 0 ? (
                    <>
                        <div style={{ textAlign: 'center', marginBottom: '10px' }}>
                            <input
                                type="text"
                                placeholder="Ieškoti..."
                                value={submittedSearchTerm}
                                onChange={(e) => setSubmittedSearchTerm(e.target.value)}
                                style={{ padding: '8px', width: '80%', maxWidth: '400px', borderRadius: '4px', border: '1px solid #ccc' }}
                            />
                        </div>
                        <table>
                            <thead>
                                <tr>
                                    <th>Vid. kodas</th>
                                    <th>Vardas</th>
                                    <th>Pavardė</th>
                                    <th>El. paštas</th>
                                    <th>Tel. nr.</th>
                                    <th>Studijų programa</th>
                                    <th>Specializacija</th>
                                    <th>Katedra</th>
                                    <th>Veiksmai</th>
                                </tr>
                            </thead>
                            <tbody>
                                {submittedUsers.map(user => {
                                    const studyProgramLabel = user.studyProgram 
                                        ? (studyPrograms.find(sp => sp.value === user.studyProgram.toString())?.label || '-') 
                                        : '-';

                                    const departmentLabel = user.department 
                                        ? (departments.find(dep => dep.value === user.department.toString())?.label || '-') 
                                        : '-';

                                    const specializationLabel = (user.specialization != null)
                                    ? (specializations.find(s => s.value === user.specialization!.toString())?.label || '-')
                                    : '-';

                                    return (
                                        <tr key={user.id}>
                                            <td>{user.id}</td>
                                            <td>{user.name}</td>
                                            <td>{user.lastName}</td>
                                            <td>{user.email}</td>
                                            <td>{user.phone}</td>
                                            <td>{studyProgramLabel}</td>
                                            <td>{specializationLabel}</td>
                                            <td>{departmentLabel}</td>
                                            <td>
                                                <button className="btn btn-outline-danger" onClick={() => handleRejectWithReason(user.id)}>Atmesti</button>
                                                <button className="btn btn-outline-primary" onClick={() => viewRejectionHistory(user.id)}>Istorija</button>
                                                <button className="btn-accept" onClick={() => handleAccept(user.id)}>Patvirtinti</button>
                                                <button className="btn-delete" onClick={() => handleDelete({ name: user.name, email: user.email, id: user.id })}>Pašalinti</button>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </>
                ) : (
                    <p>Nėra laukiančių patvirtinimo paskyrų.</p>
                )}
            </div>

            {/* Other Accounts Section */}
            <div className="table-container">
                <h3>Kitos paskyros</h3>
                {otherAccounts.length > 0 ? (
                    <>
                        <div style={{ textAlign: 'center', marginBottom: '10px' }}>
                    <input
                        type="text"
                        placeholder="Ieškoti..."
                        value={otherSearchTerm}
                        onChange={(e) => setOtherSearchTerm(e.target.value)}
                        style={{ padding: '8px', width: '80%', maxWidth: '400px', borderRadius: '4px', border: '1px solid #ccc' }}
                    />
                </div>
                    <table>
                        <thead>
                            <tr>
                                <th>Vid. kodas</th>
                                <th>Vardas</th>
                                <th>Pavardė</th>
                                <th>El. paštas</th>
                                <th>Tel. nr.</th>
                                <th>Veiksmai</th>
                            </tr>
                        </thead>
                        <tbody>
                            {otherAccounts.map(user => (
                                <tr key={user.id}>
                                    <td>{user.id}</td>
                                    <td>{user.name}</td>
                                    <td>{user.lastName}</td>
                                    <td>{user.email}</td>
                                    <td>{user.phone}</td>
                                    <td>
                                        <button className="btn btn-outline-primary history" onClick={() => viewRejectionHistory(user.id)}>Istorija</button>     
                                        <button className="btn-delete" onClick={() => handleDelete({ name: user.name, email: user.email, id: user.id})}>Pašalinti</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    </>
                    
                ) : (
                    <p>Nėra kitų paskyrų.</p>
                )}
            {eduModalOpen && educationalResults && (
                <div style={{
                    position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
                    backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex',
                    justifyContent: 'center', alignItems: 'center', zIndex: 1000
                }}>
                    <div style={{
                        backgroundColor: '#fff', padding: '20px', borderRadius: '8px',
                        maxWidth: '800px', width: '90%', maxHeight: '80vh', overflowY: 'auto',
                        position: 'relative'
                    }}>
                        <button type="button" className="close-add-event-button" onClick={() => setEduModalOpen(false)} style={{
                            position: 'absolute', top: '10px', right: '10px', background: 'none',
                            border: 'none', fontSize: '20px', cursor: 'pointer'
                        }}>
                            &times;
                        </button>
                        <h3 style={{ textAlign: 'center'}}>Ugdymo rezultatai</h3>
                        <p style={{ textAlign: 'center'}}>
                            Vidutinis įvertinimas: {educationalResults.averageRating ?? 'N/A'}
                        </p>
                        {/* Chart */}
                        <div style={{ marginBottom: '20px' }}>
                            {chartData && (
                                <Bar data={chartData} options={chartOptions} />
                            )}
                        </div>
                        {/* Data table */}
                        <table>
                            <thead>
                                <tr>
                                    <th>Užduoties pavadinimas</th>
                                    <th>Terminas</th>
                                    <th>Statusas</th>
                                    <th>Įvertinimas</th>
                                </tr>
                            </thead>
                            <tbody>
                                {educationalResults.results.map((res: any, index: number) => (
                                    <tr key={index}>
                                        <td>{res.taskName}</td>
                                        <td>{new Date(res.deadline).toISOString().split('T')[0]}</td>
                                        <td>{res.completionFile === "completed" ? 'Pateiktas' : 'Nepateiktas'}</td>
                                        <td>{res.feedbackRating ?? '-'}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        {activityStats && (
                            <div style={{ marginTop: '30px' }}>
                            <h3>Ugdytinio aktyvumas</h3>
                            <div style={{ display: 'flex', justifyContent: 'space-around', flexWrap: 'wrap' }}>
                                <div style={{ maxWidth: '300px', margin: '20px' }}>
                                <h4>Aktyvumas mokymuose</h4>
                                {trainingPieData && <Pie data={trainingPieData} options={{ responsive: true }} />}
                                </div>
                                <div style={{ maxWidth: '300px', margin: '20px' }}>
                                <h4>Aktyvumas renginiuose</h4>
                                {affairPieData && <Pie data={affairPieData} options={{ responsive: true }} />}
                                </div>
                            </div>
                            <p>
                                Mokymai, į kuriuos registravosi: {activityStats.registeredTrainings} / {activityStats.totalTrainings}<br/>
                                Renginiai, į kuriuos registravosi: {activityStats.registeredAffairs} / {activityStats.totalAffairs}
                            </p>
                            </div>
                        )}
                        <button className='btn btn-danger' onClick={() => setEduModalOpen(false)} style={{ marginTop: '20px', display: 'block', float: 'right' }}>
                            Uždaryti
                        </button>
                    </div>
                </div>
            )}
            </div>
            {/* Rejection History Modal */}

            {modalOpen && (
                <div style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
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
                        maxWidth: '500px',
                        width: '90%'
                    }}>
                        <h3>Atmetimo istorija</h3>
                        {history.length > 0 ? (
                            <ul>
                                {history.map((entry, index) => (
                                    <li key={index}>
                                        {formatRejectionDate(entry.rejectedAt)}: {entry.reason}
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p>Nėra įrašų.</p>
                        )}
                        <button className="btn-delete" style={{ marginLeft: '80%' }} onClick={() => setModalOpen(false)}>Uždaryti</button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Accounts;
