import React, { useState, useEffect } from 'react';
import { Calendar, momentLocalizer, Views } from 'react-big-calendar';
import moment from 'moment/min/moment-with-locales';
import Modal from '../Modal';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import ReactDatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css";
import { lt } from 'date-fns/locale/lt';
import './Timetable.scss';
import ICAL from 'ical.js';

moment.locale('lt');
moment.updateLocale('lt', { week: { dow: 1 } });
const localizer = momentLocalizer(moment);

const messages = {
    date: 'Data',
    time: 'Laikas',
    event: 'Įvykis',
    allDay: 'Visa diena',
    week: 'Savaitė',
    agenda: 'Tvarkaraštis',
    work_week: 'Darbo savaitė',
    day: 'Diena',
    month: 'Mėnuo',
    previous: 'Atgal',
    next: 'Pirmyn',
    yesterday: 'Vakar',
    tomorrow: 'Rytoj',
    today: 'Šiandien',
    noEventsInRange: 'Įvykių nerasta',
    showMore: (total: number) => `+${total} daugiau`,
};

const classTypeMap: Record<number, string> = {
    1: 'Laboratoriniai darbai',
    2: 'Pratybos',
    3: 'Seminaras',
    4: 'Teorija',
    5: 'Mentoriaus pridėtas užsiėmimas'
};

const generateUniqueClassCode = (): string => {
    return 'CLS' + Math.random().toString(36).substring(2, 10).toUpperCase();
};

const formats = {
    timeGutterFormat: (date: Date, culture?: string) =>
        localizer.format(date, 'HH:mm', culture),
    eventTimeRangeFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
        `${localizer.format(start, 'HH:mm', culture)} - ${localizer.format(end, 'HH:mm', culture)}`,
    agendaTimeRangeFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
        `${localizer.format(start, 'HH:mm', culture)} - ${localizer.format(end, 'HH:mm', culture)}`,
    agendaHeaderFormat: ({ start, end }: { start: Date; end: Date }, culture?: string) =>
        `${moment(start).locale('lt').format('YYYY-MM-DD')} - ${moment(end).locale('lt').format('YYYY-MM-DD')}`,
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
};

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
                        {typeof messages[view] === 'function' ? messages[view](0) : messages[view]}
                    </button>
                ))}
            </div>
        </div>
    );
};

interface Event {
    userCode: any;
    id: number;
    department: string;
    title: string;
    start: Date;
    end: Date;
    auditorium: string;
    teacher: string;
    type: string;
    code: string;
    duration: number;
}

interface TimeSlot {
    start: Date;
    end: Date;
}

// New interface for registered events
interface RegisteredEvent {
    eventId: number;
    name: string;
    start: Date;
    end: Date;
    address: string;
    comment: string;
    category: 'registered';
}

const Timetable = () => {
    const [events, setEvents] = useState<Event[]>([]);
    const [isAddEventModalOpen, setIsAddEventModalOpen] = useState(false);
    const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [selectedEvent, setSelectedEvent] = useState<Event | null>(null);
    const [startDate, setStartDate] = useState<Date | null>(null);
    const [endDate, setEndDate] = useState<Date | null>(null);
    const [suggestedSlots, setSuggestedSlots] = useState<TimeSlot[]>([]);
    const [editFormData, setEditFormData] = useState<any>({}); // holds the form data in edit mode
    const [editSuggestedSlots, setEditSuggestedSlots] = useState<TimeSlot[]>([]);
    // New state for registered events:
    const [registeredEvents, setRegisteredEvents] = useState<any[]>([]);
    // New modal states for registered events:
    const [selectedRegisteredEvent, setSelectedRegisteredEvent] = useState<any | null>(null);
    const [isRegisteredModalOpen, setIsRegisteredModalOpen] = useState(false);

    const refreshUserEvents = async () => {
        const userData = localStorage.getItem('user');
        if (userData) {
            const currentUser = JSON.parse(userData);
            const response = await fetch(`http://localhost:5216/api/UserClass/${currentUser.code}`);
            const data = await response.json();
            const formatted = data.map((uc: any) => ({
                id: uc.id || uc.id_UserClass, 
                title: uc.class?.name,
                userCode: uc.userCode,
                start: new Date(uc.startTime),
                end: new Date(uc.endTime),
                auditorium: uc.auditorium,
                teacher: uc.teacher,
                type: uc.type,
                department: uc.department,
                code: uc.classCode,
                duration: uc.duration,
            }));
            setEvents(formatted);
        }
    };

    const fetchRegisteredEvents = async () => {
        const userData = localStorage.getItem('user');
        if (!userData) return;
        const currentUser = JSON.parse(userData);
        let regEvents: any[] = [];
        try {
            const resTrainings = await fetch(`http://localhost:5216/api/events/registered/trainings?userCode=${currentUser.code}`);
            if (resTrainings.ok) {
                const trainings = await resTrainings.json();
                const mappedTrainings = trainings.map((item: any) => ({
                    eventId: item.eventId,
                    title: item.name, 
                    start: new Date(item.startTime),
                    end: new Date(item.endTime),
                    address: item.address,
                    comment: item.comment,
                    category: 'registered',
                    registeredType: 'training'
                }));
                regEvents = regEvents.concat(mappedTrainings);
            }
            const resAffairs = await fetch(`http://localhost:5216/api/events/registered/affairs?userCode=${currentUser.code}`);
            if (resAffairs.ok) {
                const affairs = await resAffairs.json();
                const mappedAffairs = affairs.map((item: any) => ({
                    eventId: item.eventId,
                    title: item.name, 
                    start: new Date(item.startTime),
                    end: new Date(item.endTime),
                    address: item.address,
                    comment: item.comment,
                    category: 'registered',
                    registeredType: 'affair'
                }));
                regEvents = regEvents.concat(mappedAffairs);
            }
            setRegisteredEvents(regEvents);
        } catch (error) {
            console.error("Error fetching registered events:", error);
        }
    };

    useEffect(() => {
        refreshUserEvents();
        fetchRegisteredEvents();
    }, []);

    // Combined events for Calendar: classes and registered events
    const allEvents = [...events, ...registeredEvents];

    // Helper: compute free time slots (logic remains unchanged)
    const getSuggestedTimes = (): TimeSlot[] => {
        const baseDate = startDate ? new Date(startDate) : new Date();
        const suggestionDate = new Date(baseDate);
        suggestionDate.setHours(9, 0, 0, 0);

        const suggestions: TimeSlot[] = [];

        for(let hour = 9; hour < 17; hour++) {
            let slotStart = new Date(suggestionDate);
            slotStart.setHours(hour);
            let slotEnd = new Date(slotStart);
            slotEnd.setHours(hour + 1);
            suggestions.push({ start: slotStart, end: slotEnd });
        }
        return suggestions;
    };

    const getSuggestedTimesFromDate = (eventsList: Event[], baseDate?: Date): TimeSlot[] => {
        const suggestions: TimeSlot[] = [];
        // Start from candidate: if a base date is provided, use that; otherwise, use now.
        let candidate = baseDate ? new Date(baseDate) : new Date();
        candidate.setHours(9, 0, 0, 0);
        
        let daysTried = 0;
        while (suggestions.length < 3 && daysTried < 7) {
            for (let hour = 9; hour < 17 && suggestions.length < 3; hour++) {
                let slotStart = new Date(candidate);
                slotStart.setHours(hour, 0, 0, 0);
                let slotEnd = new Date(slotStart);
                slotEnd.setHours(hour + 1);
                // Only suggest if the slot start is in the future.
                if (slotStart < new Date()) continue;
                // Check for overlap with any event.
                let overlapping = eventsList.some(ev => {
                    let evStart = new Date(ev.start);
                    let evEnd = new Date(ev.end);
                    return slotStart < evEnd && slotEnd > evStart;
                });
                if (!overlapping) {
                    suggestions.push({ start: slotStart, end: slotEnd });
                }
            }
            if (suggestions.length >= 3) break;
            // Move to next day.
            candidate.setDate(candidate.getDate() + 1);
            daysTried++;
        }
        return suggestions.slice(0, 3);
    };

    useEffect(() => {
        if (isAddEventModalOpen) {
            const slots = getSuggestedTimesFromDate(events, startDate || undefined);
            setSuggestedSlots(slots);
        }
    }, [isAddEventModalOpen, startDate, events]);

    useEffect(() => {
        if (isEditModalOpen) {
            const base = editFormData.startTime ? new Date(editFormData.startTime) : new Date();
            const slots = getSuggestedTimesFromDate(events, base);
            setEditSuggestedSlots(slots);
        }
    }, [isEditModalOpen, editFormData.startTime, events]);

    const handleSuggestionClick = (slot: TimeSlot) => {
        setStartDate(slot.start);
        setEndDate(slot.end);
    };

    const eventColorMapping: Record<number, string> = {
        1: '#013220',
        2: '#0000FF',
        3: '#000080',
        4: '#970F0F',
    };

    // Adjust event style based on whether it is a class or a registered event
    const eventStyleGetter = (
        event: any,
        start: Date,
        end: Date,
        isSelected: boolean
    ) => {
        if (event.category === 'registered') {
            return { style: { backgroundColor: '#2E8B57', borderRadius: '5px', opacity: 0.8, color: 'white', padding: '4px' } };
        }
        const typeNumber = Number(event.type);
        const backgroundColor = eventColorMapping[typeNumber] || (isSelected ? '#3174ad' : '#367CF7');
        const style = {
            backgroundColor,
            borderRadius: '5px',
            opacity: 0.8,
            color: 'white',
            border: '0px',
            padding: '4px',
        };
        return { style };
    };

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file || !file.name.endsWith('.ics')) {
          alert("Pasirinkite .ics formato failą.");
          return;
        }
        const reader = new FileReader();
        reader.onload = async () => {
          try {
            const fileContent = reader.result as string;
            const jcalData = ICAL.parse(fileContent);
            const vcalendar = new ICAL.Component(jcalData);
            const vevents = vcalendar.getAllSubcomponents('vevent');
      
            const userData = localStorage.getItem('user');
            if (!userData) return;
            const currentUser = JSON.parse(userData);
      
            for (const component of vevents) {
              const eventObj = new ICAL.Event(component);
              let summary = eventObj.summary || "";
              // Extract first 8 characters as class code and the rest as class name.
              const classCode = summary.substring(0, 8);
              const className = summary.substring(8).trim();
      
              // Create the class if it doesn't exist.
              const classResponse = await fetch('http://localhost:5216/api/Class', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ code: classCode, name: className })
              });
      
              if (!classResponse.ok) {
                console.error("Nepavyko sukurti užsiėmimo (klasės) su kodu:", classCode);
                continue;
              }
      
              // Prepare user class assignment.
              const start = eventObj.startDate.toJSDate();
              const end = eventObj.endDate.toJSDate();
              const duration = Math.round((end.getTime() - start.getTime()) / 60000);
      
              const userClassObj = {
                userCode: currentUser.code,
                classCode: classCode,
                // You might update department and auditorium as needed.
                department: currentUser.department || 0,
                auditorium: "",
                teacher: eventObj.location || "", // or assign teacher from another source
                type: 4, // default type; adjust as needed
                duration: duration,
                startTime: moment(start).format('YYYY-MM-DDTHH:mm:ss'),
                endTime: moment(end).format('YYYY-MM-DDTHH:mm:ss')
              };
      
              const assignResponse = await fetch('http://localhost:5216/api/UserClass/assign', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(userClassObj)
              });
      
              if (!assignResponse.ok) {
                console.error("Nepavyko priskirti užsiėmimo vartotojui, klasės kodas:", classCode);
              }
            }
      
            // Optionally refresh events after processing.
            refreshUserEvents();
          } catch (error) {
            console.error("Klaida įkraunant ICS failą:", error);
          }
        };
        reader.readAsText(file);
      };

    const handleAddEvent = () => {
        setIsAddEventModalOpen(true);
    };

    const closeAddEventModal = () => {
        setIsAddEventModalOpen(false);
    };

    // New handler for start date: update start time and set end time to same value
    const handleStartDateChange = (date: Date | null) => {
        setStartDate(date);
        if (date) {
            // Auto-set end time to same date and time as start time.
            setEndDate(date);
        }
    };

    const handleAddEventSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const form = e.currentTarget;
        let codeInput = (form.elements.namedItem('code') as HTMLInputElement).value.trim();
        const code = codeInput || generateUniqueClassCode();
        const title = (form.elements.namedItem('title') as HTMLInputElement).value;
        const department = parseInt((form.elements.namedItem('department') as HTMLInputElement).value);
        const auditorium = (form.elements.namedItem('auditorium') as HTMLInputElement).value;
        const teacher = (form.elements.namedItem('teacher') as HTMLInputElement).value;
        const type = parseInt((form.elements.namedItem('type') as HTMLSelectElement).value);

        // New repetition controls:
        const repetitionCount = parseInt((form.elements.namedItem('repetitionCount') as HTMLInputElement).value, 10) || 0;
        const repetitionInterval = parseInt((form.elements.namedItem('repetitionInterval') as HTMLInputElement).value, 10) || 0;

        if (title && startDate && endDate) {
            if (startDate >= endDate) {
                alert("Pradžios laikas turi būti anksčiau nei pabaigos laikas.");
                return;
            }
            // Calculate event duration in minutes.
            const duration = Math.round((endDate.getTime() - startDate.getTime()) / 60000);

            // Create the basic class.
            const basicClass = {
                code,
                name: title,
            };
            const classResponse = await fetch('http://localhost:5216/api/class', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(basicClass),
            });
            if (!classResponse.ok) {
                alert("Klaida įrašant klasę.");
                return;
            }
            const userData = localStorage.getItem('user');
            if (!userData) {
                alert("Nėra įrašyto vartotojo informacijos.");
                return;
            }
            const currentUser = JSON.parse(userData);
            
            // Create an array for all events (first event + repetitions)
            let eventsToSchedule = [];
            // First event.
            eventsToSchedule.push({
                userCode: currentUser.code,
                classCode: code,
                department,
                auditorium,
                teacher,
                type,
                duration,
                startTime: moment(startDate).format('YYYY-MM-DDTHH:mm:ss'),
                endTime: moment(endDate).format('YYYY-MM-DDTHH:mm:ss')
            });
            // Additional repeated events.
            for (let i = 1; i <= repetitionCount; i++) {
                // Here, repetitionInterval is taken as days.
                let newStart = new Date(startDate.getTime());
                let newEnd = new Date(endDate.getTime());
                newStart.setDate(newStart.getDate() + repetitionInterval * i);
                newEnd.setDate(newEnd.getDate() + repetitionInterval * i);
                eventsToSchedule.push({
                    userCode: currentUser.code,
                    classCode: code,
                    department,
                    auditorium,
                    teacher,
                    type,
                    duration,
                    startTime: moment(newStart).format('YYYY-MM-DDTHH:mm:ss'),
                    endTime: moment(newEnd).format('YYYY-MM-DDTHH:mm:ss')
                });
            }
            // Loop and add each user class.
            for (const eventObj of eventsToSchedule) {
                const assignResponse = await fetch('http://localhost:5216/api/UserClass/assign', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(eventObj),
                });
                if (!assignResponse.ok) {
                    // Read error text and alert the user.
                    const errorMessage = await assignResponse.text();
                    alert(`Klaida tvarkaraštyje: ${errorMessage}`);
                    return;
                }
            }
            await refreshUserEvents();
            setIsAddEventModalOpen(false);
            setStartDate(null);
            setEndDate(null);
        }
    };

    // Modify onSelectEvent: check if the event is a registered event
    const handleSelectEvent = (event: any) => {
        if (event.category === 'registered') {
            setSelectedRegisteredEvent(event);
            setIsRegisteredModalOpen(true);
        } else {
            setSelectedEvent(event);
            setIsDetailModalOpen(true);
        }
    };

    const closeDetailModal = () => {
        setIsDetailModalOpen(false);
        setSelectedEvent(null);
    };

    // Delete handler uses native window.confirm.
    const handleDeleteClick = async () => {
        if (!selectedEvent) return;
        if (window.confirm("Ar tikrai norite ištrinti šį užsiėmimą?")) {
            const response = await fetch(`http://localhost:5216/api/UserClass/${selectedEvent.id}`, {
                method: 'DELETE',
            });
            if (response.ok) {
                alert("Užsiėmimas ištrintas.");
                await refreshUserEvents();
                closeDetailModal();
            } else {
                alert("Ištrinant įvyko klaida.");
            }
        }
    };

    // Called when user clicks "Edit" in the detail modal.
    const openEditModal = () => {
        if (selectedEvent) {
            setEditFormData({
                id: selectedEvent.id,
                userCode: selectedEvent.userCode,
                title: selectedEvent.title,
                classCode: selectedEvent.code,
                department: selectedEvent.department,
                auditorium: selectedEvent.auditorium,
                teacher: selectedEvent.teacher,
                type: selectedEvent.type,
                duration: selectedEvent.duration,
                startTime: new Date(selectedEvent.start),
                endTime: new Date(selectedEvent.end)
            });
            setIsEditModalOpen(true);
        }
    };

    const handleEditInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setEditFormData((prev: any) => ({
            ...prev,
            [name]: value
        }));
    };

    // Handler for date changes in edit form
    const handleEditStartDateChange = (date: Date | null) => {
        setEditFormData((prev: any) => ({
            ...prev,
            startTime: date,
            endTime: date // auto-set end date same as start
        }));
    };

    const handleEditEndDateChange = (date: Date | null) => {
        setEditFormData((prev: any) => ({
            ...prev,
            endTime: date
        }));
    };

    const handleEditSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!editFormData.startTime || !editFormData.endTime) {
            alert("Pasirinkite pradžios ir pabaigos laikus.");
            return;
        }
        if (new Date(editFormData.startTime) >= new Date(editFormData.endTime)) {
            alert("Pradžios laikas turi būti anksčiau nei pabaiga.");
            return;
        }
        // Compute duration if needed
        editFormData.duration = Math.round((new Date(editFormData.endTime).getTime() - new Date(editFormData.startTime).getTime())/60000);

        const response = await fetch(`http://localhost:5216/api/UserClass/edit/${editFormData.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userCode: editFormData.userCode,
                classCode: editFormData.classCode,
                department: parseInt(editFormData.department),
                auditorium: editFormData.auditorium,
                teacher: editFormData.teacher,
                type: parseInt(editFormData.type),
                duration: editFormData.duration,
                startTime: moment(editFormData.startTime).format('YYYY-MM-DDTHH:mm:ss'),
                endTime: moment(editFormData.endTime).format('YYYY-MM-DDTHH:mm:ss')
            })
        });
        if (response.ok) {
            alert("Užsiėmimas sėkmingai atnaujintas.");
            await refreshUserEvents();
            setIsEditModalOpen(false);
            setSelectedEvent(null);
        } else {
            alert("Nepavyko atnaujinti užsiėmimo. Patikrinkite duomenis.");
        }
    };

    const handleEditSuggestionClick = (slot: TimeSlot) => {
        setEditFormData((prev: any) => ({
             ...prev,
             startTime: slot.start,
             endTime: slot.end
        }));
    };

    useEffect(() => {
        if (isEditModalOpen) {
            const base = editFormData.startTime ? new Date(editFormData.startTime) : new Date();
            const slots = getSuggestedTimesFromDate(events, base);
            setEditSuggestedSlots(slots);
        }
    }, [isEditModalOpen, editFormData.startTime, events]);

    const exportCalendarToICS = () => {
        // Create a new calendar component.
        const vcalendar = new ICAL.Component(['vcalendar', [], []]);
        vcalendar.updatePropertyWithValue('version', '2.0');
        vcalendar.updatePropertyWithValue('prodid', '-//NKDUPVS//Calendar Export//EN');
      
        // Iterate through events and add each as a VEVENT.
        events.forEach((event) => {
          const vevent = new ICAL.Component('vevent');
      
          // Add summary, UID, start and end.
          vevent.addPropertyWithValue('summary', event.title);
          vevent.addPropertyWithValue('uid', event.id.toString());
          vevent.addPropertyWithValue('dtstart', ICAL.Time.fromJSDate(new Date(event.start)));
          vevent.addPropertyWithValue('dtend', ICAL.Time.fromJSDate(new Date(event.end)));
          
          // Optionally add location
          if (event.auditorium) {
            vevent.addPropertyWithValue('location', event.auditorium);
          }
      
          // Append the event to the calendar.
          vcalendar.addSubcomponent(vevent);
        });
      
        // Generate the ICS string.
        const icsString = vcalendar.toString();
      
        // Create a Blob from the string and generate a downloadable URL.
        const blob = new Blob([icsString], { type: 'text/calendar;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', 'calendar_events.ics');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      };

    return (
        <div className="timetable-container">
            <div className="calendar-container">
                <Calendar
                    localizer={localizer}
                    culture="lt"
                    messages={messages}
                    formats={formats}
                    components={{ toolbar: CustomToolbar }}
                    startAccessor="start"
                    endAccessor="end"
                    style={{ height: "calc(100vh - 50px)", width: "100%", marginBottom: "30px" }}
                    events={allEvents}
                    views={[Views.MONTH, Views.WEEK, Views.DAY, Views.AGENDA]}
                    eventPropGetter={eventStyleGetter}
                    onSelectEvent={handleSelectEvent}
                />
            </div>
            <div className="controls-container">
                <div className="file-upload">
                    <label htmlFor="file-upload" className="file-upload-label">
                        Pasirinkti failą
                    </label>
                    <input
                        type="file"
                        id="file-upload"
                        className="file-upload-input"
                        onChange={handleFileChange}
                    />
                </div>
                <div className="event-buttons">
                    <button className="add-event-btn" onClick={handleAddEvent}><strong>Pridėti užsiėmimą</strong></button>
                    <button className="export-btn" onClick={exportCalendarToICS} style={{ marginTop: '10px' }}>
                        Eksportuoti kalendorių
                    </button>
                </div>
            </div>
            
            <Modal isOpen={isAddEventModalOpen} onClose={closeAddEventModal}>
                <div className="event-modal-container">
                    <button type="button" className="close-add-event-button" onClick={closeAddEventModal}>
                        &times;
                    </button>
                    <h4 className="modal-title">Pridėti užsiėmimą</h4>
                    <form onSubmit={handleAddEventSubmit} className="modal-form">
                        <div className="form-group">
                            <label htmlFor="code">Kodas:</label>
                            <input type="text" name="code" id="code" placeholder="Jeigu paliksite tuščią, sugeneruos automatiškai" maxLength={10}/>
                        </div>
                        <div className="form-group">
                            <label htmlFor="title">Pavadinimas:</label>
                            <input type="text" name="title" id="title" required maxLength={100}/>
                        </div>
                        <div className="form-group">
                            <label htmlFor="department">Rūmai (numeris):</label>
                            <input type="number" min="1" max="12" name="department" id="department" required />
                        </div>
                        <div className="form-group">
                            <label htmlFor="auditorium">Auditorija:</label>
                            <input type="text" name="auditorium" id="auditorium" required maxLength={50}/>
                        </div>
                        <div className="form-group">
                            <label htmlFor="teacher">Dėstytojas(-ai):</label>
                            <input type="text" name="teacher" id="teacher" required maxLength={50}/>
                        </div>
                        <div className="form-group">
                            <label htmlFor="type">Tipas:</label>
                            <select name="type" id="type" required>
                                <option value="1">Laboratoriniai darbai</option>
                                <option value="2">Pratybos</option>
                                <option value="3">Seminaras</option>
                                <option value="4">Teorija</option>
                            </select>
                        </div>
                        <div className="form-group">
                            <label>Pradžia:</label>
                            <ReactDatePicker
                                selected={startDate}
                                onChange={handleStartDateChange}
                                showTimeSelect
                                timeFormat="HH:mm"
                                timeIntervals={15}
                                dateFormat="Pp"
                                locale={lt}            
                                timeCaption="Laikas"
                                placeholderText="Pasirinkite pradžią"
                                className="date-picker"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label>Pabaiga:</label>
                            <ReactDatePicker
                                selected={endDate}
                                onChange={(date: Date | null) => setEndDate(date)}
                                showTimeSelect
                                timeFormat="HH:mm"
                                timeIntervals={15}
                                dateFormat="Pp"
                                locale={lt}
                                timeCaption="Laikas"
                                placeholderText="Pasirinkite pabaigą"
                                className="date-picker"
                                required
                                minDate={startDate || undefined}
                            />
                        </div>
                        {suggestedSlots.length > 0 && (
                            <div className="suggestions">
                                <p>Siūlomi laiko intervalai (nuo 9 iki 17):</p>
                                <ul>
                                    {suggestedSlots.map((slot, idx) => (
                                        <li key={idx}>
                                            <button type="button" onClick={() => handleSuggestionClick(slot)}>
                                                {moment(slot.start).format('YYYY-MM-DD HH:mm')} – {moment(slot.end).format('YYYY-MM-DD HH:mm')}
                                            </button>
                                        </li>
                                    ))}
                                </ul>
                                <button type="button" className="btn btn-outline-danger" onClick={() => setSuggestedSlots([])}>
                                    Atšaukti pasiūlymus
                                </button>
                            </div>
                        )}
                        <div className="form-group">
                            <label htmlFor="repetitionCount">Papildomai pakartoti dar kartų:</label>
                            <input type="number" name="repetitionCount" id="repetitionCount" min="0" placeholder="Pvz.: 2 (dar 2 papildomi  užsiėmimai)" />
                        </div>
                        <div className="form-group">
                            <label htmlFor="repetitionInterval">Kas kiek dienų kartoti:</label>
                            <input type="number" name="repetitionInterval" id="repetitionInterval" min="1" placeholder="Pvz.: 7 (kas 7 dienas įterpiama - kas savaitę)" />
                        </div>
                        <div className="form-actions">
                            <button type="submit" className="submit-btn">Pridėti</button>
                            <button type="button" className="cancel-btn" onClick={closeAddEventModal}>Atšaukti</button>
                        </div>
                    </form>
                </div>
            </Modal>

            {isDetailModalOpen && selectedEvent && (
                <Modal isOpen={isDetailModalOpen} onClose={closeDetailModal}>
                    <div className="event-modal-container">
                        <button type="button" className="close-add-event-button" onClick={closeDetailModal}>
                            &times;
                        </button>
                        <h4 className="modal-title">Užsiėmimo informacija</h4>
                        <div style={{ margin: '20px' }}>
                            <p><strong>Kodas:</strong> {selectedEvent.code}</p>
                            <p><strong>Pavadinimas:</strong> {selectedEvent.title}</p>
                            <p><strong>Rūmai: </strong>{selectedEvent.department}</p>
                            <p><strong>Auditorija:</strong> {selectedEvent.auditorium}</p>
                            <p><strong>Dėstytojas (-ai):</strong> {selectedEvent.teacher}</p>
                            <p>
                                <strong>Pradžia:</strong> {moment(selectedEvent.start).format("YYYY-MM-DD HH:mm")}
                            </p>
                            <p>
                                <strong>Pabaiga:</strong> {moment(selectedEvent.end).format("YYYY-MM-DD HH:mm")}
                            </p>
                            <p><strong>Trukmė:</strong> {selectedEvent.duration} min.</p>
                            <p>
                                <strong>Tipas:</strong> {classTypeMap[Number(selectedEvent.type)] || selectedEvent.type}
                            </p>
                        </div>
                        <div style={{ textAlign: 'right', marginTop: '10px' }}>
                            <>
                                <button 
                                    onClick={openEditModal} 
                                    className="btn btn-outline-primary" 
                                    style={{ marginRight: '10px' }}
                                >
                                    Redaguoti užsiėmimą
                                </button>
                                <button 
                                    onClick={handleDeleteClick} 
                                    className="btn btn-outline-danger"
                                >
                                    Ištrinti užsiėmimą
                                </button>
                            </>
                        </div>
                    </div>
                </Modal>
            )}

            {isEditModalOpen && (
                <Modal isOpen={isEditModalOpen} onClose={() => setIsEditModalOpen(false)}>
                    <div className="event-modal-container">
                        <button className="close-add-event-button" onClick={() => setIsEditModalOpen(false)}>&times;</button>
                        <h4 className="modal-title">Redaguoti užsiėmimą</h4>
                        <p style={{ fontSize: '0.85em', color: '#777', textAlign: 'center', marginBottom: '15px' }}>
                            Pastaba: "Kodas" ir "Pavadinimas" nėra redaguojami.
                        </p>
                        <form onSubmit={handleEditSubmit} className="modal-form">
                            <div className="form-group">
                                <label htmlFor="classCode">Kodas:</label>
                                <input
                                    disabled
                                    type="text"
                                    name="classCode"
                                    id="classCode"
                                    value={editFormData.classCode || ''}
                                    onChange={handleEditInputChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="title">Pavadinimas:</label>
                                <input
                                    disabled
                                    type="text"
                                    name="title"
                                    id="title"
                                    value={editFormData.title || ''}
                                    onChange={handleEditInputChange}
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="department">Rūmai (numeris):</label>
                                <input
                                    type="number"
                                    name="department"
                                    id="department"
                                    value={editFormData.department || ''}
                                    onChange={handleEditInputChange}
                                    required
                                    min={1}
                                    max={12}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="auditorium">Auditorija:</label>
                                <input
                                    type="text"
                                    name="auditorium"
                                    id="auditorium"
                                    value={editFormData.auditorium || ''}
                                    onChange={handleEditInputChange}
                                    required
                                    maxLength={50}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="teacher">Dėstytojas(-ai):</label>
                                <input
                                    type="text"
                                    name="teacher"
                                    id="teacher"
                                    value={editFormData.teacher || ''}
                                    onChange={handleEditInputChange}
                                    required
                                    maxLength={50}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="type">Tipas:</label>
                                <select
                                    name="type"
                                    id="type"
                                    value={editFormData.type || ''}
                                    onChange={handleEditInputChange}
                                    required
                                >
                                    <option value="">Pasirinkite tipą</option>
                                    {Object.entries({
                                        1: 'Laboratoriniai darbai',
                                        2: 'Pratybos',
                                        3: 'Seminaras',
                                        4: 'Teorija'
                                    }).map(([key, label]) => (
                                        <option key={key} value={key}>{label}</option>
                                    ))}
                                </select>
                            </div>
                            <div className="form-group">
                                <label>Pradžia:</label>
                                <ReactDatePicker
                                    selected={editFormData.startTime ? new Date(editFormData.startTime) : null}
                                    onChange={handleEditStartDateChange}
                                    showTimeSelect
                                    timeFormat="HH:mm"
                                    timeIntervals={15}
                                    dateFormat="yyyy-MM-dd HH:mm"
                                    locale={lt}  // Added the locale prop
                                    timeCaption="Laikas"
                                    placeholderText="Pasirinkite pradžią"
                                    className="date-picker"
                                    required
                                />
                            </div>
                            <div className="form-group">
                                <label>Pabaiga:</label>
                                <ReactDatePicker
                                    selected={editFormData.endTime ? new Date(editFormData.endTime) : null}
                                    onChange={handleEditEndDateChange}
                                    showTimeSelect
                                    timeFormat="HH:mm"
                                    timeIntervals={15}
                                    dateFormat="yyyy-MM-dd HH:mm"
                                    locale={lt}  // Added the locale prop
                                    timeCaption="Laikas"
                                    placeholderText="Pasirinkite pabaigą"
                                    className="date-picker"
                                    required
                                    minDate={editFormData.startTime ? new Date(editFormData.startTime) : undefined}
                                />
                            </div>
                            <div className="form-group">
                                <label>Pasiūlymai laiko intervalams:</label>
                                {editSuggestedSlots.length > 0 && (
                                    <div className="suggestions">
                                        <ul>
                                            {editSuggestedSlots.map((slot, idx) => (
                                                <li key={idx}>
                                                    <button type="button" onClick={() => handleEditSuggestionClick(slot)}>
                                                        {moment(slot.start).format('YYYY-MM-DD HH:mm')} – {moment(slot.end).format('YYYY-MM-DD HH:mm')}
                                                    </button>
                                                </li>
                                            ))}
                                        </ul>
                                        <button className="btn btn-outline-danger" type="button" onClick={() => setEditSuggestedSlots([])}>
                                            Atšaukti pasiūlymus
                                        </button>
                                    </div>
                                )}
                            </div>
                            <div className="form-actions">
                                <button type="submit" className="submit-btn">Atnaujinti</button>
                                <button type="button" className="cancel-btn" onClick={() => setIsEditModalOpen(false)}>
                                    Atšaukti
                                </button>
                            </div>
                        </form>
                    </div>
                </Modal>
            )}

            {isRegisteredModalOpen && selectedRegisteredEvent && (
                <Modal isOpen={isRegisteredModalOpen} onClose={() => setIsRegisteredModalOpen(false)}>
                    <div className="registered-modal-container">
                        <button type="button" className="close-add-event-button" onClick={() => setIsRegisteredModalOpen(false)}>
                            &times;
                        </button>
                        <h4 className="modal-title">
                            {selectedRegisteredEvent.registeredType === 'training'
                                ? 'Mokymų informacija'
                                : 'Renginio informacija'}
                        </h4>
                        <div style={{ margin: '20px' }}>
                            <p>Pavadinimas: {selectedRegisteredEvent.title}</p>
                            <p>
                                Laikas: {moment(selectedRegisteredEvent.start).format("YYYY-MM-DD HH:mm")}–{moment(selectedRegisteredEvent.end).format("YYYY-MM-DD HH:mm")}
                            </p>
                            <p>Vieta: {selectedRegisteredEvent.address}</p>
                            <p>Komentaras: {selectedRegisteredEvent.comment}</p>
                        </div>
                    </div>
                </Modal>
            )}
        </div>
    );
};

export default Timetable;