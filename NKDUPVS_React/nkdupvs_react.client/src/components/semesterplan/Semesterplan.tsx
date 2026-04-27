import React, { useState, useEffect, ReactNode } from 'react';
import './Semesterplan.scss';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import bookIcon from '../../assets/icons/book.png';
import pencilIcon from '../../assets/icons/pencil.png';
import calendarIcon from '../../assets/icons/calendar.png';

interface MentorSuggestion {
    mentorCode: string;
    department: number;
    mentorName: string;
    mentorLastName: string;
    mentorEmail: string;
    mentorPhoneNumber: string;
    minPriority: number;
    assignedCount: number;
}

interface DepartmentItem {
    value: string;
    label: string;
}

interface AssignedTask {
  feedbackComment: ReactNode;
  feedbackRating: null;
  id_Task: number;
  name: string;
  description: string;
  materialLink?: string;
  deadline?: string;
  isAssigned: boolean;
  isRated: boolean;
  taskTypeId?: number;
  completionFile?: string;
}

const Semesterplan: React.FC = () => {
    const navigate = useNavigate();
    const [mentorSuggestions, setMentorSuggestions] = useState<MentorSuggestion[]>([]);
    const [departmentsMap, setDepartmentsMap] = useState<Record<number, string>>({});
    const [taskTypesMap, setTaskTypesMap] = useState<Record<number, string>>({});

    // Get user from localStorage (initial login info)
    const [user, setUser] = useState<any>(() => {
        const userData = localStorage.getItem('user');
        return userData ? JSON.parse(userData) : null;
    });
    
    // State for mentee info (from server) and assigned mentor details
    const [mentee, setMentee] = useState<any>(null);
    const [assignedMentor, setAssignedMentor] = useState<any>(null);
    const [assignedTasks, setAssignedTasks] = useState<AssignedTask[]>([]);

    const [sortColumn, setSortColumn] = useState<'name' | 'deadline' | 'completion' | 'feedback' | 'taskType'>('name');
    const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
    const [expandedTaskId, setExpandedTaskId] = useState<number | null>(null);

    const handleSort = (column: 'name' | 'deadline' | 'completion' | 'feedback' | 'taskType') => {
        if (sortColumn === column) {
            setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
        } else {
            setSortColumn(column);
            setSortDirection('asc');
        }
    };

    const getIconForTaskType = (typeName: string): string | undefined => {
        if (typeName === "Dėl paskaitų vedimo") {
          return bookIcon;
        } else if (typeName === "Renginys") {
          return calendarIcon;
        } else if (typeName === "Kita") {
          return pencilIcon;
        }
        return undefined;
      };

    // Fetch task types mapping from the API
    useEffect(() => {
        fetch('http://localhost:5216/api/tasktypes')
            .then(res => res.json())
            .then((data: { id: number, name: string }[]) => {
                const map: Record<number, string> = {};
                data.forEach(tt => {
                    map[tt.id] = tt.name;
                });
                setTaskTypesMap(map);
            })
            .catch(err => console.error('Error fetching task types:', err));
    }, []);

    const sortedTasks = [...assignedTasks].sort((a, b) => {
        let aValue: any, bValue: any;
        switch(sortColumn) {
            case 'name':
                aValue = a.name.toLowerCase();
                bValue = b.name.toLowerCase();
                break;
            case 'deadline':
                aValue = a.deadline ? new Date(a.deadline) : new Date(0);
                bValue = b.deadline ? new Date(b.deadline) : new Date(0);
                break;
            case 'completion':
                aValue = a.completionFile ? 1 : 0;
                bValue = b.completionFile ? 1 : 0;
                break;
            case 'feedback':
                aValue = a.isRated ? 1 : 0;
                bValue = b.isRated ? 1 : 0;
                break;
            case 'taskType':
                aValue = a.taskTypeId != null ? (taskTypesMap[a.taskTypeId] || '') : '';
                bValue = b.taskTypeId != null ? (taskTypesMap[b.taskTypeId] || '') : '';
                break;
            default:
                aValue = a.name;
                bValue = b.name;
        }
        if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
        if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
        return 0;
    });

    const toggleExpand = (taskId: number) => {
        setExpandedTaskId(expandedTaskId === taskId ? null : taskId);
    };

    // Listen for localStorage changes
    useEffect(() => {
        const handleStorageChange = () => {
            const updatedUser = localStorage.getItem('user');
            setUser(updatedUser ? JSON.parse(updatedUser) : null);
        };
        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    // Redirect if not verified or if the user is a mentor.
    useEffect(() => {
        if (!user || user.isMentor || !user.isVerified) {
            navigate('/home');
        }
    }, [user, navigate]);

    // Fetch department data from the API on initial load
    useEffect(() => {
        fetch('http://localhost:5216/api/departments')
            .then(res => res.json())
            .then((data: DepartmentItem[]) => {
                const map: Record<number, string> = {};
                data.forEach(dept => {
                    const numVal = parseInt(dept.value, 10);
                    map[numVal] = dept.label;
                });
                setDepartmentsMap(map);
            })
            .catch(err => console.error('Error fetching departments:', err));
    }, []);

    // Fetch updated mentee info from the server
    useEffect(() => {
        if (user) {
            fetch(`http://localhost:5216/api/mentee/${user.code}`)
                .then(res => res.json())
                .then(data => setMentee(data))
                .catch(err => console.error('Error fetching mentee info:', err));
        }
    }, [user]);

    // When mentee info is available, decide which branch to use
    useEffect(() => {
        if (mentee) {
            if (mentee.mentorCode) {
                // Fetch assigned mentor details
                fetch(`http://localhost:5216/api/mentor/${mentee.mentorCode}`)
                    .then(res => res.json())
                    .then(data => setAssignedMentor(data))
                    .catch(err => console.error('Error fetching assigned mentor:', err));
                setMentorSuggestions([]);
            } else {
                // Fetch mentor suggestions when no mentor is assigned
                fetch(`http://localhost:5216/api/mentor/suggestions/${mentee.code}`)
                    .then(res => res.json())
                    .then((data: MentorSuggestion[]) => setMentorSuggestions(data))
                    .catch(err => console.error('Error fetching suggestions:', err));
                setAssignedMentor(null);
            }
        }
    }, [mentee]);

    useEffect(() => {
        if (user) {
            fetch(`http://localhost:5216/api/task/assignedTasks/${user.code}`)
                .then(res => res.json())
                .then((data: AssignedTask[]) => setAssignedTasks(data))
                .catch(err => console.error("Error fetching assigned tasks:", err));
        }
    }, [user]);

    const sendRequest = (mentorCode: string) => {
        const payload = {
            menteeCode: mentee.code,
            mentorCode: mentorCode,
            requestDate: new Date().toISOString(),
            status: "Laukiama patvirtinimo"
        };

        fetch('http://localhost:5216/api/mentor/sendRequest', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(async res => {
                if (res.ok) {
                    alert("Prašymas išsiųstas.");
                } else {
                    const errorMsg = await res.text();
                    alert(errorMsg);
                }
            })
            .catch(err => console.error(err));
    };

    const markTaskCompleted = (taskId: number) => {
        // Ask the mentee for a file link or simply confirm completion.
        const completionLink = prompt("Įveskite nuorodą į įkeltą failą (palikite tuščią, jei tik pažymėti kaip atliktą):");
        const payload = { TaskId: taskId, MenteeCode: user.code, CompletionFile: completionLink || "" };
        
        fetch("http://localhost:5216/api/task/complete", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        })
        .then(res => {
            if (res.ok) {
                // Optionally re-fetch assigned tasks to update the view.
                fetch(`http://localhost:5216/api/task/assignedTasks/${user.code}`)
                    .then(res => res.json())
                    .then((data: AssignedTask[]) => setAssignedTasks(data))
                    .catch(err => console.error("Error fetching assigned tasks:", err));
            }
            else {
                alert("Klaida pažymint užduotį kaip įvykdytą.");
            }
        })
        .catch(err => console.error(err));
    };

    return (
        <div className="semesterplan">
            <h2>Semestro planas</h2>
            {mentee && mentee.mentorCode ? (
                assignedMentor ? (
                    <p>
                        Jums paskirtas mentorius yra: {assignedMentor.name} {assignedMentor.lastName}
                    </p>
                ) : (
                    <p>Loading assigned mentor info...</p>
                )
            ) : (
                <div className="tasks-section">
                    {mentorSuggestions.length > 0 ? (
                        <div className="mentor-suggestions-container">
                            <h3>Pasiūlymai galimiems mentoriams</h3>
                            <table className="mentor-suggestions-table">
                                <thead>
                                    <tr>
                                        <th>Mentoriaus kodas</th>
                                        <th>Vardas</th>
                                        <th>Pavardė</th>
                                        <th>El. paštas</th>
                                        <th>Tel. nr.</th>
                                        <th>Katedra</th>
                                        <th>Ugdytinių skaičius</th>
                                        <th>Veiksmai</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {mentorSuggestions.map((s, index) => (
                                        <tr key={`${s.mentorCode}_${index}`}>
                                            <td>{s.mentorCode}</td>
                                            <td>{s.mentorName}</td>
                                            <td>{s.mentorLastName}</td>
                                            <td>{s.mentorEmail}</td>
                                            <td>{s.mentorPhoneNumber}</td>
                                            <td>{departmentsMap[s.department] || s.department}</td>
                                            <td>{s.assignedCount}</td>
                                            <td>
                                                <button className="btn-suggestion" onClick={() => sendRequest(s.mentorCode)}>
                                                    Siųsti užklausą
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <p>Šiuo metu nėra mentorių pasiūlymų.</p>
                    )}
                </div>
            )}
            <h3>Jums paskirtos užduotys</h3>
            {assignedTasks.length > 0 ? (
                <div className="tasks-section">
                    <table className="mentor-suggestions-table">
                        <thead>
                            <tr>
                                <th onClick={() => handleSort('name')}>
                                    Pavadinimas {sortColumn === 'name' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
                                </th>
                                <th onClick={() => handleSort('deadline')}>
                                    Terminas {sortColumn === 'deadline' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
                                </th>
                                <th onClick={() => handleSort('taskType')}>
                                    Užduoties tipas {sortColumn === 'taskType' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
                                </th>
                                <th onClick={() => handleSort('completion')}>
                                    Atlikta {sortColumn === 'completion' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
                                </th>
                                <th onClick={() => handleSort('feedback')}>
                                    Ar pridėtas įvertinimas {sortColumn === 'feedback' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedTasks.map(task => (
                                <React.Fragment key={task.id_Task}>
                                    <tr style={{ cursor: "pointer" }} onClick={() => toggleExpand(task.id_Task)}>
                                        <td>{task.name}</td>
                                        <td>{task.deadline ? format(new Date(task.deadline), 'yyyy-MM-dd HH:mm') : '-'}</td>
                                        <td>
                                            {task.taskTypeId != null ? (() => {
                                                const taskTypeName = taskTypesMap[task.taskTypeId] || '-';
                                                const icon = getIconForTaskType(taskTypeName);
                                                return (
                                                <span>
                                                    {icon && (
                                                    <img
                                                        src={icon}
                                                        alt={taskTypeName}
                                                        className="task-type-icon"
                                                        style={{width: '40px', height: '40px', verticalAlign: 'middle', marginRight: '5px' }}
                                                    />
                                                    )}
                                                    {taskTypeName}
                                                </span>
                                                );
                                            })() : '-'}
                                         </td>
                                        <td>{task.completionFile ? 'Taip' : 'Ne'}</td>
                                        <td>{task.isRated ? 'Taip' : 'Ne'}</td>
                                    </tr>
                                    {expandedTaskId === task.id_Task && (
                                        <tr>
                                            <td colSpan={5}>
                                                <div className="task-card">
                                                    <p>{task.description}</p>
                                                    <p>
                                                        <strong>Medžiaga:</strong>{" "}
                                                        {task.materialLink ? (
                                                            <a href={task.materialLink} target="_blank" rel="noopener noreferrer">
                                                                {task.materialLink}
                                                            </a>
                                                        ) : (
                                                            "-"
                                                        )}
                                                    </p>
                                                    <p>
                                                        <strong>Atlikta:</strong> {task.completionFile ? 'Taip' : 'Ne'}
                                                    </p>
                                                    <p>
                                                        <strong>Įkeltas failas:</strong>
                                                        <br />
                                                        {task.completionFile && task.completionFile !== "completed" ? (
                                                            <a href={task.completionFile} target="_blank" rel="noopener noreferrer">
                                                                {task.completionFile}
                                                            </a>
                                                        ) : (
                                                            "Nepateiktas"
                                                        )}
                                                    </p>
                                                    {!task.completionFile && (
                                                        <button className='btn-suggestion' onClick={() => markTaskCompleted(task.id_Task)}>
                                                            Užduotis atlikta
                                                        </button>
                                                    )}
                                                    {task.isRated && task.feedbackRating != null && (
                                                    <div className="feedback-display">
                                                        <hr />
                                                        <h4>Įvertinimas:</h4>
                                                        <p><strong>Įvertinimas:</strong> {task.feedbackRating}</p>
                                                        <p><strong>Komentaras:</strong> {task.feedbackComment}</p>
                                                    </div>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    )}
                                </React.Fragment>
                            ))}
                        </tbody>
                    </table>
                </div>
            ) : (
                <p>Jums nepaskirtos užduotys.</p>
            )}
        </div>
    );
};

export default Semesterplan;