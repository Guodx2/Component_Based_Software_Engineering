import React, { useState, useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import Header from './components/header/Header';
import Footer from './components/footer/Footer';
import NavMenu from './components/navmenu/NavMenu';
import HomePage from './components/homePage/HomePage';
import Accounts from './components/accounts/Accounts';
import Prisijungimas from './components/login/Login';
import Timetable from './components/timetable/Timetable';
import { GoogleOAuthProvider } from '@react-oauth/google';
import ProtectedRoute from './components/ProtectedRoute';
import Notification from './components/Notification';
import PersonalInfo from './components/PersonalInfo/PersonalInfo';
import Semesterplan from './components/semesterplan/Semesterplan';
import Tasks from './components/tasks/Tasks';
import MyMentees from './components/myMentees/MyMentees';
import MentorNotification from './components/MentorNotification';
import Events from './components/events/Events'; 
import './App.css';

const App: React.FC = () => {
  const [user, setUser] = useState<any>(null);
  const [showNotification, setShowNotification] = useState(false);

  // Run this on mount only (for example, to set user from localStorage)
  useEffect(() => {
    const userData = localStorage.getItem('user');
    if (userData) {
      const parsedUser = JSON.parse(userData);
      setUser(parsedUser);
    }
  }, []);

  // Watch for changes in the user state and update showNotification accordingly.
  useEffect(() => {
    if (user && !user.isSubmitted) {
      setShowNotification(true);
    } else {
      setShowNotification(false);
    }
  }, [user]);

  const handleFormSubmit = (data: any) => {
    const userData = localStorage.getItem('user');
    if (!userData) return;
    const currentUser = JSON.parse(userData);
  
    let endpoint = '';
    if (data.userType === 'mentee') {
      endpoint = 'http://localhost:5216/api/user/update/mentee';
    } else if (data.userType === 'mentor') {
      endpoint = 'http://localhost:5216/api/user/update/mentor';
    }
  
    const payload: any = {
      code: currentUser.code,
      phoneNumber: data.phoneNumber,
    };
  
    if (data.userType === 'mentee') {
      payload.studyProgram = parseInt(data.studyProgram, 10);
      if (data.specialization) {  // add specialization if exists
        payload.specialization = parseInt(data.specialization, 10);
      }
    } else if (data.userType === 'mentor') {
      payload.department = parseInt(data.department, 10);
    }
  
    fetch(endpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    }).then(response => {
      if (response.ok) {
        // Update user so that isSubmitted is true
        const updatedUser = { ...currentUser, isSubmitted: true };
        localStorage.setItem('user', JSON.stringify(updatedUser));
        setUser(updatedUser);
      }
    });
  };

  return (
    <GoogleOAuthProvider clientId="114677274660-0b6upac95821fvl8m12sscgtdbig4dk4.apps.googleusercontent.com">
      <Header />
      {user && <NavMenu user={user} setUser={setUser} setShowNotification={setShowNotification} />}
      {user && user.isMentor && (
        <MentorNotification mentorCode={user.code} />
      )}
      <div id="root">
        {showNotification && (
          <Notification message="Prašome pateikti daugiau duomenų apie save" onClose={() => setShowNotification(false)} onSubmit={handleFormSubmit} />
        )}
        <Routes>
          <Route path="/" element={<Prisijungimas setUser={setUser} setShowNotification={setShowNotification} />} />
          <Route path="/home" element={<ProtectedRoute user={user} component={HomePage} />} />
          <Route path='/semesterplan' element={<ProtectedRoute user={user} component={Semesterplan} nonAdminOnly/>} />          
          <Route path='/mymentees' element={<ProtectedRoute user={user} component={MyMentees} />} />
          <Route path="/tasks" element={<ProtectedRoute user={user} component={Tasks} />} />
          <Route path="/accounts" element={<ProtectedRoute user={user} component={Accounts} adminOnly={true} />} />          
          <Route path="/timetable" element={<ProtectedRoute user={user} component={Timetable} />} />
          <Route path="/personalinfo" element={<ProtectedRoute user={user} component={PersonalInfo} />} />
          <Route path="/events" element={<ProtectedRoute user={user} component={Events} />} />
        </Routes>
      </div>
      <Footer />
    </GoogleOAuthProvider>
  );
}

export default App;