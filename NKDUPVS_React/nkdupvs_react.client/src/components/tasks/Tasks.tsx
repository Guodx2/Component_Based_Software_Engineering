import React, { useState, useEffect, FormEvent } from 'react';
import ReactDatePicker from 'react-datepicker';
import { lt } from 'date-fns/locale';
import { format, formatISO } from 'date-fns';
import "react-datepicker/dist/react-datepicker.css";
import './Tasks.scss';
import { useNavigate } from 'react-router-dom';
import bookIcon from '../../assets/icons/book.png';
import calendarIcon from '../../assets/icons/calendar.png';
import pencilIcon from '../../assets/icons/pencil.png';

interface Task {
    id_Task: number;
    name: string;
    description: string;
    materialLink?: string;
    deadline?: string;
    isAssigned: boolean;
    isRated: boolean;
    createdBy: string;
    taskTypeId?: number;
}

interface TaskType {
    id: number;
    name: string;
}

interface Mentee {
    code: string;
    name: string;
    lastName: string;
}

interface TaskCardProps {
    task: Task;
    typeName: string;
    onEdit: (task: Task) => void;
    onDelete: (id: number) => void;
    availableMentees: Mentee[]; // pass in the full mentee list
    searchTerm: string; // pass the search term as a prop
}
const TaskCard: React.FC<TaskCardProps> = ({ task, typeName, onEdit, onDelete, availableMentees, searchTerm }) => {
    const [assignedMentees, setAssignedMentees] = useState<string[]>([]);

    useEffect(() => {
        if (task.isAssigned) {
            fetch(`http://localhost:5216/api/task/assignments/${task.id_Task}`)
                .then(res => res.json())
                .then((data: string[]) => setAssignedMentees(data))
                .catch(error =>
                    console.error("Error fetching assigned mentees: ", error)
                );
        }
    }, [task]);

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

    // Map assigned mentee codes to full names (if available)
    const displayAssignedMentees = assignedMentees.map(code => {
        const mentee = availableMentees.find(m => m.code === code);
        return mentee ? `${mentee.name} ${mentee.lastName}` : code;
    });

    return (
        <div className="task-card">
            <div className="task-header">
                {getIconForTaskType(typeName) && (
                    <img src={getIconForTaskType(typeName)} alt="ikonėlė" className="task-icon" />
                )}
                <h3 className="task-title">{task.name}</h3>
            </div>
            <br></br>
            <br></br>
            <p className='task-description'>{task.description}</p>
            <p>
                <strong>Medžiaga:</strong>
                <br />
                {task.materialLink ? (
                    <a
                        href={task.materialLink}
                        target="_blank"
                        rel="noopener noreferrer"
                    >
                        {task.materialLink}
                    </a>
                ) : (
                    "-"
                )}
            </p>
            <p>
                <strong>Tipas:</strong> {typeName || '-'}{" "}
            </p>
            <div className="task-meta">
                <span>
                    <strong>Terminas:</strong>{" "}
                    {task.deadline ? format(new Date(task.deadline), "yyyy-MM-dd HH:mm") : '-'}
                </span>
                <span>
                    <strong>Įvertinta:</strong>{" "}
                    {task.isRated ? 'Taip' : 'Ne'}
                </span>
            </div>
            {task.isAssigned && assignedMentees.length > 0 && (
                <p>
                    <strong>Paskirta: </strong> {displayAssignedMentees.join(", ")}
                </p>
            )}
            <button className="edit-button" onClick={() => onEdit(task)}>
                Redaguoti
            </button>
            <button className="delete-button" onClick={() => onDelete(task.id_Task)}>
                Ištrinti
            </button>
        </div>
    );
};

const Tasks: React.FC = () => {
    const [tasks, setTasks] = useState<Task[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [modalOpen, setModalOpen] = useState<boolean>(false);
    const [editMode, setEditMode] = useState<boolean>(false);
    const [currentTask, setCurrentTask] = useState<Task | null>(null);
    const [newTaskName, setNewTaskName] = useState<string>('');
    const [newTaskDescription, setNewTaskDescription] = useState<string>('');
    const [newTaskMaterialLink, setNewTaskMaterialLink] = useState<string>('');
    const [newTaskDeadline, setNewTaskDeadline] = useState<Date | null>(null);
    const [newTaskTypeId, setNewTaskTypeId] = useState<number | undefined>(undefined);
    const [taskTypes, setTaskTypes] = useState<TaskType[]>([]);
    const [availableMentees, setAvailableMentees] = useState<Mentee[]>([]);
    const [selectedMentees, setSelectedMentees] = useState<string[]>([]);
    const [searchTerm, setSearchTerm] = useState<string>('');
    const [currentPage, setCurrentPage] = useState<number>(1);
    const pageSize = 6;

    const navigate = useNavigate();
    const userData = localStorage.getItem('user');
    const user = userData ? JSON.parse(userData) : null;

    useEffect(() => {
        if (!user || !user.isMentor) {
            navigate('/home'); 
        }
    }, [user, navigate]);

    const fetchTasks = () => {
        if (user) {
            fetch(`http://localhost:5216/api/task/user/${user.code}`)
                .then((res) => res.json())
                .then((data) => {
                    setTasks(data);
                    setLoading(false);
                })
                .catch((error) => {
                    console.error('Error fetching tasks:', error);
                    setLoading(false);
                });
        } else {
            setLoading(false);
        }
    };

    const fetchTaskTypes = () => {
        fetch(`http://localhost:5216/api/tasktypes`)
            .then(res => res.json())
            .then((data: TaskType[]) => setTaskTypes(data))
            .catch(err => console.error('Error fetching task types:', err));
    };

    const fetchMentees = () => {
        fetch(`http://localhost:5216/api/mentor/mentees/${user.code}`)
            .then(res => res.json())
            .then((data: Mentee[]) => {
                const validData = data.filter(m => m.code);
                setAvailableMentees(validData);
            })
            .catch(err => console.error('Error fetching mentees:', err));
    };

    useEffect(() => {
        fetchTasks();
        fetchTaskTypes();
        fetchMentees();
    }, []);

    const filteredTasks = tasks.filter(task => 
        task.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        task.description.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const displayedTasks = filteredTasks.slice((currentPage - 1) * pageSize, currentPage * pageSize);
    const totalPages = Math.ceil(filteredTasks.length / pageSize);

    const assignTaskToMentees = (taskId: number) => {
        // Always send the payload, even if selectedMentees is an empty array.
        const payload = {
            TaskId: taskId,
            MentorCode: user.code,
            MenteeCodes: selectedMentees.filter(code => !!code)
        };

        fetch('http://localhost:5216/api/task/assign', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(res => {
            if (!res.ok) {
                console.error("Error assigning task.");
            } else {
                // Refresh tasks to update UI immediately.
                fetchTasks();
            }
        })
        .catch(err => console.error(err));
    };

    const handleAddTask = (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!user) return;

        const deadlineString = newTaskDeadline
            ? formatISO(newTaskDeadline)
            : null;

        const newTask = {
            name: newTaskName,
            description: newTaskDescription,
            materialLink: newTaskMaterialLink,
            deadline: deadlineString,
            createdBy: user.code,
            isAssigned: false,
            isRated: false,
            taskTypeId: newTaskTypeId
        };

        fetch('http://localhost:5216/api/task/create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newTask)
        })
            .then(async res => {
                if (res.ok) {
                    const createdTask: Task = await res.json();
                    assignTaskToMentees(createdTask.id_Task);
                    setModalOpen(false);
                    setNewTaskName('');
                    setNewTaskDescription('');
                    setNewTaskMaterialLink('');
                    setNewTaskDeadline(null);
                    setNewTaskTypeId(undefined);
                    setSelectedMentees([]);
                    fetchTasks();
                } else {
                    console.error('Error creating task.');
                }
            })
            .catch((error) => console.error('Error:', error));
    };

    const handleUpdateTask = (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!user || !currentTask) return;

        const deadlineString = newTaskDeadline
            ? formatISO(newTaskDeadline)
            : null;

        // Include menteeCodes (use a lowercase key to match the C# model)
        const updatedTask = {
            ...currentTask,
            name: newTaskName,
            description: newTaskDescription,
            materialLink: newTaskMaterialLink,
            deadline: deadlineString,
            taskTypeId: newTaskTypeId,
            menteeCodes: selectedMentees  // Added assignment payload
        };

        fetch(`http://localhost:5216/api/task/update/${currentTask.id_Task}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedTask)
        })
            .then(async res => {
                if (res.ok) {
                    // If update is successful, reassign task and update UI.
                    assignTaskToMentees(currentTask.id_Task);
                    setModalOpen(false);
                    setEditMode(false);
                    setCurrentTask(null);
                    setNewTaskName('');
                    setNewTaskDescription('');
                    setNewTaskMaterialLink('');
                    setNewTaskDeadline(null);
                    setNewTaskTypeId(undefined);
                    setSelectedMentees([]);
                    fetchTasks();
                } else {
                    console.error('Error updating task.');
                }
            })
            .catch((error) => console.error('Error:', error));
    };

    const handleEditTask = (task: Task) => {
        setEditMode(true);
        setCurrentTask(task);
        setNewTaskName(task.name);
        setNewTaskDescription(task.description);
        setNewTaskMaterialLink(task.materialLink || '');
        setNewTaskDeadline(task.deadline ? new Date(task.deadline) : null);
        setNewTaskTypeId(task.taskTypeId);

        fetch(`http://localhost:5216/api/task/assignments/${task.id_Task}`)
            .then(res => res.json())
            .then((data: string[]) => {
                setSelectedMentees(data);
            })
            .catch(error => console.error("Error fetching assigned mentees: ", error));

        setModalOpen(true);
    };

    const handleDeleteTask = (taskId: number) => {
        if (!user) return;
        if (window.confirm("Ar tikrai norite ištrinti šią užduotį?")) {
            fetch(`http://localhost:5216/api/task/delete/${taskId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            })
                .then(async (res) => {
                    if (res.ok) {
                        fetchTasks();
                    } else {
                        const errorMessage = await res.text();
                        alert(errorMessage);
                    }
                })
                .catch((error) => console.error('Error:', error));
        }
    };

    const currentTaskTypeName = (taskTypeId?: number) => {
        return taskTypes.find(tt => tt.id === taskTypeId)?.name || '-';
    };

    return (
        <div className="uzduotys-page">
            <h2>Mano kurtos užduotys</h2>
            <div className="task-search" style={{ marginBottom: '20px', textAlign: 'center' }}>
                <input
                    type="text"
                    placeholder="Ieškoti užduoties..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    style={{ padding: '8px', width: '80%', maxWidth: '400px', borderRadius: '4px', border: '1px solid #ccc' }}
                />
            </div>
            <div className="add-task-button">
                <button className="btn btn-success" onClick={() => { setModalOpen(true); setEditMode(false); }}>
                    Pridėti užduotį
                </button>
            </div>
            {loading ? (
                <p>Kraunama...</p>
            ) : filteredTasks.length > 0 ? (
                <>
                  <div className="tasks-grid">
                      {displayedTasks.map((task: Task) => (
                          <TaskCard
                              key={`task_${task.id_Task}`}
                              task={task}
                              typeName={currentTaskTypeName(task.taskTypeId)}
                              onEdit={handleEditTask}
                              onDelete={handleDeleteTask}
                              availableMentees={availableMentees}
                              searchTerm={searchTerm}
                          />
                      ))}
                  </div>
                  {totalPages > 1 && (
                      <div style={{ marginTop: '1rem', textAlign: 'center' }}>
                          <button 
                              className="btn btn-primary-outline" 
                              onClick={() => setCurrentPage(currentPage - 1)} 
                              disabled={currentPage === 1}
                          >
                              Ankstesni
                          </button>
                          <span style={{ margin: '0 1rem' }}>{currentPage} / {totalPages}</span>
                          <button 
                              className="btn btn-primary-outline" 
                              onClick={() => setCurrentPage(currentPage + 1)} 
                              disabled={currentPage === totalPages}
                          >
                              Kiti
                          </button>
                      </div>
                  )}
                </>            
            ) : (
                <p>Užduočių nerasta.</p>
            )}

            {/* Task Modal Form */}
            {modalOpen && (
                <div className="modal-backdrop" onClick={() => setModalOpen(false)}>
                    <div className="event-modal-container" onClick={(e) => e.stopPropagation()}>
                        <button type="button" className="close-add-event-button" onClick={() => setModalOpen(false)}>
                            &times;
                        </button>
                        <h4 className="modal-title">
                            {editMode ? 'Redaguoti užduotį' : 'Pridėti užduotį'}
                        </h4>
                        <form onSubmit={editMode ? handleUpdateTask : handleAddTask} className="modal-form">
                            <div className="form-group">
                                <label>Užduoties pavadinimas:</label>
                                <input
                                    type="text"
                                    value={newTaskName}
                                    onChange={(e) => setNewTaskName(e.target.value)}
                                    required
                                    maxLength={50}
                                />
                            </div>
                            <div className="form-group">
                                <label>Aprašymas:</label>
                                <textarea
                                    style={{ width: '100%', maxHeight: '200px' }}
                                    value={newTaskDescription}
                                    onChange={(e) => setNewTaskDescription(e.target.value)}
                                    maxLength={255}
                                ></textarea>
                            </div>
                            <div className="form-group">
                                <label>Medžiagos nuoroda:</label>
                                <input
                                    type="text"
                                    value={newTaskMaterialLink}
                                    onChange={(e) => setNewTaskMaterialLink(e.target.value)}
                                />
                            </div>
                            <div className="form-group">
                                <label>Terminas:</label>
                                <ReactDatePicker
                                    selected={newTaskDeadline}
                                    onChange={(date: Date | null) => setNewTaskDeadline(date)}
                                    showTimeSelect
                                    timeFormat="HH:mm"
                                    timeCaption="Laikas"
                                    timeIntervals={15}
                                    dateFormat="Pp"
                                    locale={lt}
                                    placeholderText="Pasirinkite terminą"
                                    className="date-picker"
                                />
                            </div>
                            <div className="form-group">
                                <label>Užduoties tipas:</label>
                                <select 
                                    value={newTaskTypeId || ''} 
                                    onChange={(e) => setNewTaskTypeId(Number(e.target.value))}
                                    required
                                >
                                    <option value="" disabled>Pasirinkite tipą</option>
                                    {taskTypes.map((tt) => (
                                        <option key={`tt_${tt.id}`} value={tt.id}>
                                            {tt.name}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            {/* Assignment section */}
                            <div className="form-group">
                                <label>Priskirti užduotį ugdytiniams:</label>
                                {availableMentees.length > 0 ? (
                                    availableMentees.map((mentee, index) => (
                                        <div key={`${mentee.code}_${index}`}>
                                            <input
                                                type="checkbox"
                                                value={mentee.code}
                                                checked={selectedMentees.includes(mentee.code)}
                                                onChange={(e) => {
                                                    if (e.target.checked) {
                                                        setSelectedMentees(prev => [...prev, mentee.code]);
                                                    } else {
                                                        setSelectedMentees(prev => prev.filter(code => code !== mentee.code));
                                                    }
                                                }}
                                            />
                                            <span>{mentee.name} {mentee.lastName}</span>
                                        </div>
                                    ))
                                ) : (
                                    <p>Nėra ugdytinių</p>
                                )}
                            </div>
                            <div className="form-actions">
                                <button type="submit" className="submit-btn">
                                    {editMode ? 'Atnaujinti' : 'Išsaugoti'}
                                </button>
                                <button type="button" className="cancel-btn" onClick={() => setModalOpen(false)}>
                                    Atšaukti
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Tasks;