import React, { useEffect, useState, useRef } from 'react';
import moment from 'moment';
import Modal from '../Modal';

interface MentorEvent {
    id: number;
    title: string;
    start: Date;
    end: Date;
    auditorium: string;
    teacher: string;
    type: number;
    code: string;
    duration: number;
}

interface AddMenteeEventModalProps {
    menteeCode: string;
    mentorCode: string;
    onClose: () => void;
    onEventAdded: () => void;
}

const AddMenteeEventModal: React.FC<AddMenteeEventModalProps> = ({
    menteeCode,
    mentorCode,
    onClose,
    onEventAdded
}) => {
    const [mentorEvents, setMentorEvents] = useState<MentorEvent[]>([]);
    const [selectedEvent, setSelectedEvent] = useState<MentorEvent | null>(null);
    const [repetitionCount, setRepetitionCount] = useState<number>(0);
    const [repetitionInterval, setRepetitionInterval] = useState<number>(0);
    const [searchTerm, setSearchTerm] = useState<string>('');
    const [showSuggestions, setShowSuggestions] = useState<boolean>(false);
    const searchRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        fetch(`http://localhost:5216/api/UserClass/${mentorCode}`)
            .then(res => res.json())
            .then((data: any[]) => {
                const events = data.map(ev => ({
                    id: ev.id || ev.id_UserClass,
                    title: ev.class?.name || ev.title,
                    start: new Date(ev.startTime),
                    end: new Date(ev.endTime),
                    auditorium: ev.auditorium,
                    teacher: ev.teacher,
                    type: ev.type,
                    code: ev.classCode,
                    duration: ev.duration,
                }));
                setMentorEvents(events);
            })
            .catch(err => console.error(err));
    }, [mentorCode]);

    const filteredEvents = mentorEvents.filter(ev =>
        ev.title.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const handleSuggestionClick = (ev: MentorEvent) => {
        setSelectedEvent(ev);
        setSearchTerm(`${ev.title} (${moment(ev.start).format('YYYY MM DD HH:mm')}-${moment(ev.end).format('YYYY MM DD HH:mm')})`);        setShowSuggestions(false);
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!selectedEvent) {
            alert("Pasirinkite užsiėmimą iš pasiūlymų.");
            return;
        }
        const eventsToSchedule = [];
        eventsToSchedule.push({
            userCode: menteeCode,
            classCode: selectedEvent.code,
            department: selectedEvent.type,
            auditorium: selectedEvent.auditorium,
            teacher: selectedEvent.teacher,
            type: 5, // Mentor-added event
            duration: selectedEvent.duration,
            startTime: moment(selectedEvent.start).format('YYYY-MM-DDTHH:mm:ss'),
            endTime: moment(selectedEvent.end).format('YYYY-MM-DDTHH:mm:ss'),
        });
        for (let i = 1; i <= repetitionCount; i++) {
            const newStart = moment(selectedEvent.start).add(repetitionInterval * i, 'days').toDate();
            const newEnd = moment(selectedEvent.end).add(repetitionInterval * i, 'days').toDate();
            eventsToSchedule.push({
                userCode: menteeCode,
                classCode: selectedEvent.code,
                department: selectedEvent.type,
                auditorium: selectedEvent.auditorium,
                teacher: selectedEvent.teacher,
                type: 5,
                duration: selectedEvent.duration,
                startTime: moment(newStart).format('YYYY-MM-DDTHH:mm:ss'),
                endTime: moment(newEnd).format('YYYY-MM-DDTHH:mm:ss'),
            });
        }
        for (const evt of eventsToSchedule) {
            const response = await fetch('http://localhost:5216/api/UserClass/assign', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(evt),
            });
            if (!response.ok) {
                const errorText = await response.text();
                alert(`Klaida: ${errorText}`);
                return;
            }
        }
        onEventAdded();
        onClose();
    };

    return (
        <Modal isOpen={true} onClose={onClose}>
            <div className="event-modal-container" style={{ width: '400px', height: 'auto', margin: '-40px' }}>
                <h4 className="modal-title">Pridėti užsiėmimą ugdytiniui</h4>
                <button type="button" className="close-add-event-button" onClick={onClose}>
                    &times;
                </button>
                <form onSubmit={handleSubmit} className="modal-form">
                    <div className="form-group" style={{ position: 'relative' }}>
                        <label htmlFor="classSearch">Ieškoti užsiėmimo:</label>
                        <input
                            type="text"
                            id="classSearch"
                            placeholder="Įveskite pavadinimą..."
                            value={searchTerm}
                            ref={searchRef}
                            onChange={(e) => {
                                setSearchTerm(e.target.value);
                                setShowSuggestions(true);
                                setSelectedEvent(null);
                            }}
                            onFocus={() => setShowSuggestions(true)}
                            onBlur={() => setTimeout(() => setShowSuggestions(false), 150)}
                        />
                        {showSuggestions && (
                            <ul
                                style={{
                                    position: 'absolute',
                                    top: '100%',
                                    left: 0,
                                    right: 0,
                                    background: 'white',
                                    border: '1px solid #ccc',
                                    maxHeight: '150px',
                                    overflowY: 'auto',
                                    zIndex: 10,
                                    margin: 0,
                                    padding: '5px',
                                    listStyle: 'none'
                                }}
                            >
                                {filteredEvents.map(ev => (
                                    <li
                                        key={ev.id}
                                        style={{ padding: '5px', cursor: 'pointer' }}
                                        onMouseDown={() => handleSuggestionClick(ev)}
                                    >
                                        {ev.title} ({moment(ev.start).format('YYYY-MM-DD HH:mm')})
                                    </li>
                                ))}
                                {filteredEvents.length === 0 && (
                                    <li style={{ padding: '5px' }}>Nėra rezultatų</li>
                                )}
                            </ul>
                        )}
                    </div>
                    <div className="form-group">
                        <label htmlFor="repetitionCount">Papildomų pakartojimų skaičius:</label>
                        <input
                            type="number"
                            id="repetitionCount"
                            name="repetitionCount"
                            min="0"
                            placeholder="Pvz.: 2"
                            value={repetitionCount}
                            onChange={(e) => setRepetitionCount(Number(e.target.value))}
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="repetitionInterval">Intervalas (dienomis):</label>
                        <input
                            type="number"
                            id="repetitionInterval"
                            name="repetitionInterval"
                            min="1"
                            placeholder="Pvz.: 7"
                            value={repetitionInterval}
                            onChange={(e) => setRepetitionInterval(Number(e.target.value))}
                        />
                    </div>
                    <div className="form-actions">
                        <button type="submit" className="submit-btn">Pridėti užsiėmimą</button>
                        <button type="button" className="cancel-btn" onClick={onClose}>Atšaukti</button>
                    </div>
                </form>
            </div>
        </Modal>
    );
};

export default AddMenteeEventModal;