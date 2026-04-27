import React, { useEffect, useState } from 'react';
import { Calendar, momentLocalizer, Messages } from 'react-big-calendar';
import moment from 'moment/min/moment-with-locales';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import Modal from '../Modal';
import AddMenteeClassModal from './AddMenteeClassModal';
import './MenteeTimeTableModal.scss';

export interface Event {
    id: number;
    title: string;
    start: Date;
    end: Date;
    auditorium?: string;
    teacher?: string;
    type?: number;
    code?: string;
    duration?: number;
}

interface ClassType {
    id_ClassTypes: number;
    name: string;
}

// Hardcoded default class types mapping for front-end use
const defaultClassTypes: ClassType[] = [
    { id_ClassTypes: 1, name: 'Laboratoriniai darbai' },
    { id_ClassTypes: 2, name: 'Pratybos' },
    { id_ClassTypes: 3, name: 'Seminaras' },
    { id_ClassTypes: 4, name: 'Teorija' },
    { id_ClassTypes: 5, name: 'Mentoriaus pridėtas užsiėmimas' }
];

interface MenteeTimetableModalProps {
    menteeCode: string;
    onClose: () => void;
}

moment.locale('lt'); // set moment to Lithuanian
const localizer = momentLocalizer(moment);

// Lithuanian translations for Calendar messages
const messages: Messages = {
    allDay: 'Visą dieną',
    previous: 'Atgal',
    next: 'Pirmyn',
    today: 'Šiandien',
    month: 'Mėnuo',
    week: 'Savaitė',
    day: 'Diena',
    agenda: 'Tvarkaraštis',
    date: 'Data',
    time: 'Laikas',
    event: 'Įvykis',
    showMore: total => `+${total} daugiau`,
    noEventsInRange: 'Nėra užsiėmimų', 
};

// Custom formats for military time (24-hour clock)
const formats = {
  timeGutterFormat: (date: Date, culture?: string) =>
    localizer.format(date, 'HH:mm', culture),
  eventTimeRangeFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
    `${localizer.format(start, 'HH:mm', culture)} - ${localizer.format(end, 'HH:mm', culture)}`,
  agendaTimeRangeFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
    `${localizer.format(start, 'HH:mm', culture)} - ${localizer.format(end, 'HH:mm', culture)}`,
  agendaHeaderFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
    `${moment(start).locale('lt').format('YYYY MM DD')} - ${moment(end).locale('lt').format('YYYY MM DD')}`,
  agendaDateFormat: (date: Date, culture?: string) =>
    moment(date).locale('lt').format('YYYY-MM-DD'),
  dayHeaderFormat: (date: Date) =>
    moment(date).locale('lt').format('dddd, MMMM DD'),
  weekdayFormat: (date: Date, culture?: string) =>
    moment(date).locale('lt').format('ddd'),
  monthHeaderFormat: (date: Date, culture?: string) =>
    moment(date).locale('lt').format('YYYY MMMM'),
  dayRangeHeaderFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
    `${moment(start).locale('lt').format('YYYY MMMM DD')} - ${moment(end).locale('lt').format('DD')}`,
  dayFormat: (date: Date, culture?: string) =>
    moment(date).locale('lt').format('ddd'),
  toolbarFormat: (date: Date, culture?: string) =>
    moment(date).locale('lt').format('MMMM, YYYY'),
};

// Define a custom toolbar with the order: Atgal, Šiandien, Pirmyn
const CustomToolbar = (toolbar: any) => {
    const goToBack = () => toolbar.onNavigate('PREV');
    const goToNext = () => toolbar.onNavigate('NEXT');
    const goToCurrent = () => toolbar.onNavigate('TODAY');
    const handleViewChange = (view: string) => toolbar.onView(view);
  
    return (
        <div className="rbc-toolbar">
            <button className="left-btn" onClick={goToBack}>Atgal</button>
            <button className="middle-btn" onClick={goToCurrent}>Šiandien</button>
            <button className="right-btn" onClick={goToNext}>Pirmyn</button>
            <span className="rbc-toolbar-label">{toolbar.label}</span>
            <div className="rbc-btn-group">
                {toolbar.views.map((view: keyof typeof messages) => (
                    <button
                        key={view}
                        type="button"
                        className={view === toolbar.view ? 'rbc-active' : ''}
                        onClick={() => handleViewChange(view)}
                    >
                        {typeof messages[view] === 'function' ? messages[view](0, [], []) : messages[view]}
                    </button>
                ))}
            </div>
        </div>
    );
};

// Add color mapping and event style getter as in Timetable
const eventColorMapping: Record<number, string> = {
    1: '#013220',
    2: '#0000FF',
    3: '#000080',
    4: '#970F0F',
};

const eventStyleGetter = (event: Event) => {
    const typeNumber = Number(event.type);
    const backgroundColor = eventColorMapping[typeNumber] || '#3174ad';
    return {
        style: {
            backgroundColor,
            borderRadius: '5px',
            opacity: 0.8,
            color: 'white',
            border: '0px',
            padding: '4px',
        }
    };
};

const MenteeTimetableModal: React.FC<MenteeTimetableModalProps> = ({ menteeCode, onClose }) => {
    if (!menteeCode || !onClose) return null; // Ensure valid props
    const [events, setEvents] = useState<Event[]>([]);
    const [selectedEvent, setSelectedEvent] = useState<Event | null>(null);
    const [showAddEventModal, setShowAddEventModal] = useState<boolean>(false);
    // Use default mapping since we don't have backend data for class types
    const [classTypes] = useState<ClassType[]>(defaultClassTypes);
    
    const mentorCode = localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')!).code : '';

    const fetchEvents = () => {
        fetch(`http://localhost:5216/api/UserClass/${menteeCode}`)
            .then(res => res.json())
            .then((data: any[]) => {
                const mapped = data.map(ev => ({
                    id: ev.id || ev.id_UserClass,
                    title: ev.class?.name || 'Užsiėmimas',
                    start: new Date(ev.startTime),
                    end: new Date(ev.endTime),
                    auditorium: ev.auditorium,
                    teacher: ev.teacher,
                    type: ev.type,
                    code: ev.classCode,
                    duration: ev.duration,
                }));
                setEvents(mapped);
            })
            .catch(err => console.error(err));
    };

    // Helper function to get class type name
    const getClassTypeName = (typeId?: number) => {
        const id = Number(typeId);
        const found = classTypes.find(t => t.id_ClassTypes === id);
        return found ? found.name : "Nežinomas tipas";
    };

    useEffect(() => {
        fetchEvents();
    }, [menteeCode]);

    const handleSelectEvent = (event: Event) => {
        setSelectedEvent(event);
    };

    const closeDetailModal = () => {
        setSelectedEvent(null);
    };

    return (
        <div className="modal-backdrop" onClick={onClose}>
            <div className="timetable-content" onClick={e => e.stopPropagation()}>
                <button className="close-button" onClick={onClose}>&times;</button>
                <h3>Ugdytinio Tvarkaraštis</h3>
                <button style={{ width: '50%', marginLeft: '25%' }} onClick={() => setShowAddEventModal(true)} className="export-btn">
                    Pridėti užsiėmimą
                </button>
                <Calendar
                    localizer={localizer}
                    events={events}
                    startAccessor="start"
                    endAccessor="end"
                    messages={messages}
                    formats={formats}
                    culture="lt"
                    components={{ toolbar: CustomToolbar }}
                    onSelectEvent={handleSelectEvent}
                    eventPropGetter={eventStyleGetter}
                    style={{ height: 'auto', minHeight: 500 }}
                />
            </div>
            {selectedEvent && (
                <Modal isOpen={true} onClose={closeDetailModal}>
                    <div className="event-modal-container" style={{ width: '400px', height: 'auto', margin: '-40px' }}>
                        <button type="button" className="close-add-event-button" onClick={closeDetailModal}>
                            &times;
                        </button>
                        <h4 className="modal-title">Užsiėmimo informacija</h4>
                        <div style={{ margin: '20px' }}>
                            <p><strong>Kodas:</strong> {selectedEvent.code}</p>
                            <p><strong>Pavadinimas:</strong> {selectedEvent.title}</p>
                            <p><strong>Auditorija:</strong> {selectedEvent.auditorium}</p>
                            <p><strong>Dėstytojas:</strong> {selectedEvent.teacher}</p>
                            <p><strong>Pradžia:</strong> {moment(selectedEvent.start).format("YYYY-MM-DD HH:mm")}</p>
                            <p><strong>Pabaiga:</strong> {moment(selectedEvent.end).format("YYYY-MM-DD HH:mm")}</p>
                            <p><strong>Trukmė:</strong> {selectedEvent.duration} min.</p>
                            {/* Use helper to display class type name */}
                            <p><strong>Tipas:</strong> {getClassTypeName(selectedEvent.type)}</p>
                        </div>
                    </div>
                </Modal>
            )}
            {showAddEventModal && mentorCode && (
                <AddMenteeClassModal
                    menteeCode={menteeCode}
                    mentorCode={mentorCode}
                    onClose={() => setShowAddEventModal(false)}
                    onEventAdded={fetchEvents}
                />
            )}
        </div>
    );
};

export default MenteeTimetableModal;