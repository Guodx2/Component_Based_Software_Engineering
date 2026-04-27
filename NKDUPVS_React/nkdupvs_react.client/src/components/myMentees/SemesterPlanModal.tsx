import React, { useState, useEffect, useRef } from 'react';
import { format } from 'date-fns';
import clamp from 'clamp-js';
import './SemesterPlanModal.scss';
import '../semesterplan/Semesterplan.scss';
import FeedbackModal from '../FeedbackModal';
import TaskModal from './TaskModal';
import { TaskType } from './TaskModal'; // adjust import if necessary
import { Mentee } from './TaskModal';
import bookIcon from '../../assets/icons/book.png';
import pencilIcon from '../../assets/icons/pencil.png';
import calendarIcon from '../../assets/icons/calendar.png';

export interface AssignedTask {
  id_Task: number;
  name: string;
  description: string;
  materialLink?: string;
  deadline?: string;
  completionFile?: string;
  semesterPlanTaskId: number;
  feedbackRating?: number | null;
  feedbackComment?: string | null;
  taskTypeId?: number; 
}

interface MenteeDetails {
  name: string;
  lastName: string;
}

interface SemesterPlanModalProps {
  menteeCode: string;
  onClose: () => void;
}

const TaskDescription: React.FC<{ text: string }> = ({ text }) => {
  return <p className="task-description">{text}</p>;
};

const SemesterPlanModal: React.FC<SemesterPlanModalProps> = ({ menteeCode, onClose }) => {
  const [assignedTasks, setAssignedTasks] = useState<AssignedTask[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [menteeDetails, setMenteeDetails] = useState<MenteeDetails | null>(null);
  const [feedbackTaskId, setFeedbackTaskId] = useState<number | null>(null);
  const [feedbackEditData, setFeedbackEditData] = useState<{ rating: number; comment: string } | null>(null);
  const [taskModalOpen, setTaskModalOpen] = useState<boolean>(false);
  const [taskModalData, setTaskModalData] = useState<Partial<AssignedTask> | null>(null);
  const mentorCode = "Z5506";

  // New states for the dropdown lists
  const [availableTaskTypes, setAvailableTaskTypes] = useState<TaskType[]>([]);
  const [availableMentees, setAvailableMentees] = useState<Mentee[]>([]);

  // New states for table sorting/expansion – similar to Semesterplan.tsx
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

  const toggleExpand = (taskId: number) => {
    setExpandedTaskId(expandedTaskId === taskId ? null : taskId);
  };

  const fetchTasks = () => {
    fetch(`http://localhost:5216/api/task/assignedTasks/${menteeCode}`)
      .then(res => res.json())
      .then((data: AssignedTask[]) => {
        setAssignedTasks(data);
        setLoading(false);
      })
      .catch(err => {
        console.error("Error fetching semester plan tasks:", err);
        setLoading(false);
      });
  };

  // Example fetches for task types and mentees. Replace URLs with your actual endpoints.
  const fetchTaskTypes = () => {
    fetch(`http://localhost:5216/api/task/taskTypes`)
      .then(res => res.json())
      .then((data: TaskType[]) => setAvailableTaskTypes(data))
      .catch(err => console.error("Error fetching task types:", err));
  };

  const fetchMentees = () => {
    fetch(`https://localhost:7124/api/mentor/mentees/${mentorCode}`)
      .then(res => res.json())
      .then((data: Mentee[]) => {
        console.log('Fetched mentees:', data);
        setAvailableMentees(data);
      })
      .catch(err => console.error("Error fetching mentees:", err));
  };

  useEffect(() => {
    fetchTasks();
    fetchTaskTypes();
    fetchMentees();
  }, [menteeCode]);

  useEffect(() => {
    fetch(`http://localhost:5216/api/mentor/mentee/${menteeCode}`)
      .then(res => res.json())
      .then(data => {
        setMenteeDetails({ name: data.name, lastName: data.lastName });
      })
      .catch(err => console.error("Error fetching mentee details:", err));
  }, [menteeCode]);

  const handleDeleteTask = (id_Task: number) => {
    if (window.confirm("Ar tikrai norite ištrinti šią užduotį?")) {
      fetch(`http://localhost:5216/api/task/delete/${id_Task}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' }
      })
        .then(async res => {
          if (res.ok) {
            fetchTasks();
          } else {
            const errorMessage = await res.text();
            alert(errorMessage);
          }
        })
        .catch(err => console.error(err));
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

  const sortedTasks = [...assignedTasks].sort((a, b) => {
    let aValue: any, bValue: any;
    switch (sortColumn) {
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
        aValue = a.feedbackRating ? 1 : 0;
        bValue = b.feedbackRating ? 1 : 0;
        break;
      case 'taskType':
        aValue = a.taskTypeId != null
          ? (availableTaskTypes.find(tt => tt.id === a.taskTypeId)?.name || '')
          : '';
        bValue = b.taskTypeId != null
          ? (availableTaskTypes.find(tt => tt.id === b.taskTypeId)?.name || '')
          : '';
        break;
      default:
        aValue = a.name;
        bValue = b.name;
    }
    if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
    if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
    return 0;
  });

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '90%' }}>
        <button className="close-button" onClick={onClose}>&times;</button>
        <h3>
          Semestro planas:{" "}
          {menteeDetails ? `${menteeDetails.name} ${menteeDetails.lastName}` : menteeCode}
        </h3>

        <div style={{ textAlign: 'center', marginBottom: '20px' }}>
          <button className="btn-success" style={{ float: 'right', marginBottom: '20px' }} onClick={() => { setTaskModalData(null); setTaskModalOpen(true); }}>
            Pridėti užduotį
          </button>
          <br /><br />
        </div>

        {loading ? (
          <p>Kraunama...</p>
        ) : assignedTasks.length > 0 ? (
          <div className="tasks-section">
            <table className="semesterplan-modal-table">
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
                  <th>Veiksmai</th>
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
                          const taskType = availableTaskTypes.find(tt => tt.id === task.taskTypeId);
                          if(taskType) {
                            const icon = getIconForTaskType(taskType.name);
                            return (
                              <span>
                                {icon && (
                                  <img
                                    src={icon}
                                    alt={taskType.name}
                                    style={{ width: '40px', height: '40px', verticalAlign: 'middle', marginRight: '5px' }}
                                  />
                                )}
                                {taskType.name}
                              </span>
                            );
                          } else {
                            return '-';
                          }
                        })() : '-'}
                      </td>
                      <td>{task.completionFile ? 'Taip' : 'Ne'}</td>
                      <td>{task.feedbackRating ? 'Taip' : 'Ne'}</td>
                      <td onClick={e => e.stopPropagation()}>
                        {!task.completionFile && (
                          <>
                            <button className="btn-success" onClick={() => { setTaskModalData(task); setTaskModalOpen(true); }}>
                              Redaguoti
                            </button>
                            <button className="btn-danger-outline" onClick={() => handleDeleteTask(task.id_Task)}>
                              Ištrinti
                            </button>
                          </>
                        )}
                        {task.completionFile && !task.feedbackRating && (
                          <button className="btn-secondary-outline" onClick={() => setFeedbackTaskId(task.semesterPlanTaskId)}>
                            Palikti atsiliepimą
                          </button>
                        )}
                      </td>
                    </tr>
                    {expandedTaskId === task.id_Task && (
                      <tr>
                        <td colSpan={6}>
                          <div className="task-card">
                          <div className="description-wrapper">
                            <TaskDescription text={task.description} />
                          </div>                          
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
                            {task.feedbackRating && (
                              <div className="feedback-display">
                                <hr />
                                <h4>Įvertinimas:</h4>
                                <p><strong>Balas:</strong> {task.feedbackRating}</p>
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
          <p>Nėra priskirtų užduočių.</p>
        )}

        {feedbackTaskId && (
          <FeedbackModal
            semesterPlanTaskId={feedbackTaskId}
            onClose={() => setFeedbackTaskId(null)}
            onFeedbackSubmitted={() => {
              fetchTasks();
              setFeedbackTaskId(null);
              setFeedbackEditData(null);
            }}
            isEdit={feedbackEditData !== null}
            initialRating={feedbackEditData?.rating ?? 1}
            initialComment={feedbackEditData?.comment ?? ''}
          />
        )}

        {taskModalOpen && (
          <TaskModal
            mentorCode={mentorCode}
            onClose={() => { setTaskModalOpen(false); setTaskModalData(null); }}
            onTaskSubmitted={() => {
              fetchTasks();
              setTaskModalOpen(false);
              setTaskModalData(null);
            }}
            initialTask={taskModalData
              ? {
                  ...taskModalData,
                  name: taskModalData.name || '',
                  description: taskModalData.description || ''
                }
              : undefined}
            menteeCode={menteeCode}
            availableTaskTypes={availableTaskTypes}
            availableMentees={availableMentees}
          />
        )}
      </div>
    </div>
  );
};

export default SemesterPlanModal;