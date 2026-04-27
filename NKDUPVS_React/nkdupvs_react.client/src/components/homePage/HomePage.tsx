import React, { useState, useEffect } from 'react';
import moment from 'moment';
import './HomePage.scss';
import Student from '../../Student.png';
import Time from '../../Time.png';
import ToDo from '../../ToDo.png';

function HomePage() {
  const [nextEvent, setNextEvent] = useState<any>(null);
  const [mentorMenteeCount, setMentorMenteeCount] = useState<number>(0);
  const [mentorInfo, setMentorInfo] = useState<any>(null);
  const [notRatedTasksCount, setNotRatedTasksCount] = useState<number>(0);
  const [user, setUser] = useState<any>(null);
  const [incompleteTasksCount, setIncompleteTasksCount] = useState<number>(0);

  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (userData) {
      const currentUser = JSON.parse(userData);
      setUser(currentUser);
      
      // Fetch next event
      fetch(`http://localhost:5216/api/UserClass/${currentUser.code}`)
        .then(res => res.json())
        .then(data => {
          const now = new Date();
          const upcoming = data.filter((uc: any) => new Date(uc.startTime) > now);
          upcoming.sort((a: any, b: any) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
          if (upcoming.length > 0) {
            setNextEvent(upcoming[0]);
          }
        })
        .catch(error => console.error("Error fetching events:", error));
      
      if (currentUser.isMentor === '1' || currentUser.isMentor === true || currentUser.isAdmin === true) {
        // For mentor or admin: fetch mentee count
        fetch(`http://localhost:5216/api/mentor/mentees/${currentUser.code}`)
          .then(res => res.json())
          .then(data => {
            setMentorMenteeCount(data.length);
          })
          .catch(error => console.error("Error fetching mentor mentees:", error));
        
        // For mentors: fetch not rated tasks count
        fetch(`http://localhost:5216/api/mentor/notRatedTasks/${currentUser.code}`)
          .then(res => res.json())
          .then(data => {
            setNotRatedTasksCount(data); // data is the count
          })
          .catch(error => console.error("Error fetching not rated tasks count:", error));
      
      } else {
        // For mentees: fetch the number of incomplete tasks.
        fetch(`http://localhost:5216/api/mentee/incompleteTasks/${currentUser.code}`)
          .then(res => res.json())
          .then(data => {
            setIncompleteTasksCount(data);  // data is the count of incomplete tasks
          })
          .catch(error => console.error("Error fetching incomplete tasks count:", error));
      }
    }
  }, []);

  return (
    <div className="homepage">
      {user && (user.isMentor === '1' || user.isMentor === true || user.isAdmin === true) ? (
        <>
          <div className="korta">
            <h3>Mano ugdytinių skaičius:</h3>
            <p>{mentorMenteeCount}</p>
            <img className="card-image" src={Student} alt="Student"/>
          </div>
          <div className="korta">
            <h3>Neįvertintų užduočių:</h3>
            <p>{notRatedTasksCount}</p>
            <img className="card-image" src={Time} alt="Time"/>
          </div>
        </>
      ) : (
        <>
          <div className="korta">
            <h3>Mano mentorius:</h3>
            {mentorInfo ? (
              <p>{mentorInfo.name} {mentorInfo.lastName}</p>
            ) : (
              <p>Nenurodyta</p>
            )}
            <img className="card-image" src={Student} alt="Mentor"/>
          </div>
          <div className="korta">
          <h3>Neatliktų užduočių:</h3>
          <p>{incompleteTasksCount}</p>
          <img className="card-image" src={Time} alt="Time"/>
          </div>
        </>
      )}
      <div className="korta">
        <h3>Artimiausias įvykis:</h3>
        {nextEvent ? (
          <p>
            {nextEvent.class?.name || 'Nenurodyta'} - {moment(nextEvent.startTime).format('YYYY-MM-DD HH:mm')}
          </p>
        ) : (
          <p>Nėra artimiausių įvykių.</p>
        )}
        <img className="card-image" src={ToDo} alt="ToDo"/>
      </div>
    </div>
  );
}

export default HomePage;