import React, { useState, useEffect } from 'react';
import { formatISO } from 'date-fns';
import ReactDatePicker, { registerLocale } from 'react-datepicker';
import {lt}from 'date-fns/locale/lt';
import 'react-datepicker/dist/react-datepicker.css';
import './TaskModal.scss';

registerLocale('lt', lt);

export interface TaskType {
  id: number;
  name: string;
}

export interface Mentee {
  code: string;
  name?: string;
  lastName?: string;
}

interface TaskModalProps {
  onClose: () => void;
  onTaskSubmitted: () => void;
  initialTask?: {
    id_Task?: number;
    name: string;
    description: string;
    materialLink?: string;
    deadline?: string;
    taskTypeId?: number;
  };
  menteeCode?: string;
  mentorCode: string;
  availableTaskTypes: TaskType[];
  availableMentees: Mentee[];
}

const TaskModal: React.FC<TaskModalProps> = ({
  onClose,
  onTaskSubmitted,
  initialTask,
  menteeCode,
  mentorCode,
  availableTaskTypes,
  availableMentees,
}) => {
  const [name, setName] = useState<string>(initialTask?.name || '');
  const [description, setDescription] = useState<string>(initialTask?.description || '');
  const [materialLink, setMaterialLink] = useState<string>(initialTask?.materialLink || '');
  const [deadlineDate, setDeadlineDate] = useState<Date | null>(
    initialTask?.deadline ? new Date(initialTask.deadline) : null
  );
  const [selectedTaskType, setSelectedTaskType] = useState<number | undefined>(
    initialTask?.taskTypeId
  );
  const [selectedMentees, setSelectedMentees] = useState<string[]>([]);

  useEffect(() => {
    setName(initialTask?.name || '');
    setDescription(initialTask?.description || '');
    setMaterialLink(initialTask?.materialLink || '');
    setDeadlineDate(initialTask?.deadline ? new Date(initialTask.deadline) : null);
    setSelectedTaskType(initialTask?.taskTypeId);
  }, [initialTask]);

  useEffect(() => {
    if (initialTask && initialTask.id_Task) {
      fetch(`http://localhost:5216/api/task/assignments/${initialTask.id_Task}`)
        .then(res => res.json())
        .then((data: string[]) => {
          setSelectedMentees(data);
        })
        .catch(err => console.error("Error fetching task assignments:", err));
    }
  }, [initialTask]);

  const handleSubmit = () => {
    const taskPayload = {
      name,
      description,
      materialLink,
      deadline: deadlineDate ? formatISO(deadlineDate) : null,
      createdBy: mentorCode,
      isAssigned: selectedMentees.length > 0,
      taskTypeId: selectedTaskType,
      menteeCodes: selectedMentees,
    };

    if (initialTask && initialTask.id_Task) {
      fetch(`http://localhost:5216/api/task/update/${initialTask.id_Task}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(taskPayload),
      })
        .then((res) => {
          if (res.ok) {
            onTaskSubmitted();
            onClose();
          } else {
            console.error('Error updating task.');
          }
        })
        .catch((err) => console.error(err));
    } else {
      fetch('http://localhost:5216/api/task/createForMentees', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(taskPayload),
      })
        .then((res) => {
          if (res.ok) {
            onTaskSubmitted();
            onClose();
          } else {
            console.error('Error creating task.');
          }
        })
        .catch((err) => console.error(err));
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="event-modal-container" onClick={(e) => e.stopPropagation()}>
        <button type="button" className="close-add-event-button" onClick={onClose}>
          &times;
        </button>
        <h4 className="modal-title">{initialTask ? 'Redaguoti užduotį' : 'Sukurti užduotį'}</h4>
        <form
          className="modal-form"
          onSubmit={(e) => {
            e.preventDefault();
            handleSubmit();
          }}
        >
          <div className="form-group">
            <label>Užduoties pavadinimas:</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              maxLength={50}
            />
          </div>
          <div className="form-group">
            <label>Aprašymas:</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
              maxLength={255}
            ></textarea>
          </div>
          <div className="form-group">
            <label>Medžiagos nuoroda:</label>
            <input
              type="text"
              value={materialLink}
              onChange={(e) => setMaterialLink(e.target.value)}
            />
          </div>
          <div className="form-group">
            <label>Terminas:</label>
            <ReactDatePicker
              selected={deadlineDate}
              onChange={(date: Date | null) => setDeadlineDate(date)}
              showTimeSelect
              timeFormat="HH:mm"
              locale="lt"
              timeCaption="Laikas"
              timeIntervals={15}
              dateFormat="yyyy MMMM dd, HH:mm"
              placeholderText="Pasirinkite terminą"
              className="date-picker"
            />
          </div>
          <div className="form-group">
            <label>Užduoties tipas:</label>
            <select
              value={selectedTaskType || ''}
              onChange={(e) => setSelectedTaskType(Number(e.target.value))}
              required
            >
              <option value="" disabled>
                Pasirinkite užduoties tipą
              </option>
              {availableTaskTypes.map((tt) => (
                <option key={tt.id} value={tt.id}>
                  {tt.name}
                </option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label>Priskirti ugdytiniams:</label>
            {availableMentees.length > 0 ? (
              availableMentees.map((mentee) => {
                const fullName =
                  [mentee.name, mentee.lastName].filter(text => text && text.trim().length > 0).join(" ") ||
                  mentee.code;
                return (
                  <div key={mentee.code} className="mentee-item">
                    <input
                      type="checkbox"
                      id={mentee.code}
                      value={mentee.code}
                      checked={selectedMentees.includes(mentee.code)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setSelectedMentees((prev) => [...prev, mentee.code]);
                        } else {
                          setSelectedMentees((prev) =>
                            prev.filter((code) => code !== mentee.code)
                          );
                        }
                      }}
                    />
                    <label htmlFor={mentee.code}>{fullName}</label>
                  </div>
                );
              })
            ) : (
              <p>Nėra ugdytinių</p>
            )}
          </div>
          <div className="modal-buttons">
            <button type="submit" className="submit-btn">
              Išsaugoti
            </button>
            <button type="button" className="cancel-btn" onClick={onClose}>
              Atšaukti
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TaskModal;