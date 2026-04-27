import React, { useState } from 'react';
import './PersonalInfo.scss';
import { useNavigate } from 'react-router-dom';
import MentorExpertiseForm from './MentorExpertiseForm';
import MentorExpertiseDisplay from './MentorExpertiseDisplay';

interface PersonalInfoUpdateRequest {
  Code: string;
  Name: string;
  LastName: string;
  PhoneNumber: string;
}

const PersonalInfo: React.FC = () => {
  const storedUser = localStorage.getItem('user');
  const initialUser = storedUser ? JSON.parse(storedUser) : {};
  const [user, setUser] = useState<any>(initialUser);
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [isEditingExpertise, setIsEditingExpertise] = useState<boolean>(false);
  const [name, setName] = useState<string>(user.name || '');
  const [lastName, setLastName] = useState<string>(user.lastName || '');
  const [phoneNumber, setPhoneNumber] = useState<string>(user.phoneNumber || '');
  const [acceptingMentees, setAcceptingMentees] = useState<boolean>(user.acceptingMentees ?? true);
  const navigate = useNavigate();

  const handleSave = async () => {
    // Validation and update logic…
    if (name.trim().length < 2) {
      alert("Vardas turi būti bent 2 simbolių ilgio");
      return;
    }
    if (lastName.trim().length < 2) {
      alert("Pavardė turi būti bent 2 simbolių ilgio");
      return;
    }
    if (phoneNumber.trim().length < 7 || phoneNumber.trim().length > 15) {
      alert("Telefono numeris turi būti tarp 7 ir 15 simbolių");
      return;
    }

    const updateRequest: PersonalInfoUpdateRequest = {
      Code: user.code,
      Name: name,
      LastName: lastName,
      PhoneNumber: phoneNumber
    };

    const response = await fetch('http://localhost:5216/api/user/update/personal', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(updateRequest)
    });

    if (response.ok) {
      const updatedUser = { ...user, name, lastName, phoneNumber };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      setUser(updatedUser);
      setIsEditing(false);
    } else {
      const errorData = await response.json();
      alert('Nepavyko atnaujinti asmeninių duomenų: ' + JSON.stringify(errorData));
    }
  };

  const updateMentorAvailability = async () => {
    const payload = {
      MentorCode: user.code,
      AcceptingMentees: acceptingMentees
    };

    const response = await fetch('http://localhost:5216/api/mentor/updateAvailability', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    
    if (response.ok) {
      const updatedUser = { ...user, acceptingMentees };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      setUser(updatedUser);
      alert('Nustatymas atnaujintas.');
    } else {
      alert('Nepavyko atnaujinti nustatymo.');
    }
  };

  return (
    <div className="personal-info-page">
      <h2>Asmeniniai duomenys</h2>
      
      <div className="personal-info-container">
        {isEditing ? (
          <>
            <p>
              <strong>Vidinis kodas:</strong> {user.code}
            </p>
            <p>
              <strong>Vardas:</strong>
              <input 
                type="text" 
                value={name} 
                onChange={(e) => setName(e.target.value)}
                maxLength={20}
                required
              />
            </p>
            <p>
              <strong>Pavardė:</strong>
              <input 
                type="text" 
                value={lastName} 
                onChange={(e) => setLastName(e.target.value)}
                maxLength={30}
                required
              />
            </p>
            <p>
              <strong>Tel. numeris:</strong>
              <input 
                type="text" 
                value={phoneNumber} 
                onChange={(e) => setPhoneNumber(e.target.value)}
                pattern="^\+3706\d{7}$"
                title="Tel. numeris turi būti formatu: +3706xxxxxxx"
                required
              />
            </p>
            <div style={{ marginTop: '20px', display: 'flex', gap: '10px' }}>
              <button onClick={handleSave} className="btn btn-save-edit">Išsaugoti</button>
              <button onClick={() => setIsEditing(false)} className="btn btn-decline-edit">Atšaukti</button>
            </div>
          </>
        ) : (
          <>
            <p><strong>Vidinis kodas:</strong> {user.code}</p>
            <p><strong>Vardas:</strong> {user.name}</p>
            <p><strong>Pavardė:</strong> {user.lastName}</p>
            <p><strong>Vartotojo vardas:</strong> {user.username}</p>
            <p><strong>El. paštas:</strong> {user.email}</p>
            <p><strong>Tel. numeris:</strong> {user.phoneNumber}</p>
            <p><strong>Administratorius:</strong> {user.isAdmin ? 'Taip' : 'Ne'}</p>
            <p><strong>Patvirtintas:</strong> {user.isVerified ? 'Taip' : 'Ne'}</p>
            <p><strong>Pateiktas:</strong> {user.isSubmitted ? 'Taip' : 'Ne'}</p>
            <button onClick={() => setIsEditing(true)} className="btn btn-edit-personal-info" style={{ marginTop: '20px' }}>
              Redaguoti duomenis
            </button>
          </>
        )} 
      </div>
      {user && user.isMentor && (
        <>
          <div className="mentor-availability-section" style={{ marginTop: '20px', maxWidth: '600px', width: '100%' }}>
            <h3>Prieinamumas ugdytiniams</h3>
            <label style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <input 
                type="checkbox" 
                checked={acceptingMentees} 
                onChange={(e) => setAcceptingMentees(e.target.checked)}
              />
              Priimu naujų ugdytinių
            </label>
            <button onClick={updateMentorAvailability} className="btn btn-save-edit" style={{ marginTop: '10px' }}>
              Atnaujinti
            </button>
          </div>

          <div className="mentor-expertise-section">
            {isEditingExpertise ? (
              <div className="expertise-edit-container">
                <MentorExpertiseForm 
                  mentorCode={user.code} 
                  onSuccess={() => {
                    setIsEditingExpertise(false);
                    // Optionally refresh data without reloading the full page:
                    window.location.reload();
                  }}
                />
                <button 
                  onClick={() => setIsEditingExpertise(false)} 
                  className="btn btn-close-edit"
                >
                  Uždaryti
                </button>
              </div>
            ) : (
              <div className="expertise-display-container">
                <MentorExpertiseDisplay mentorCode={user.code} />
                <button 
                  onClick={() => setIsEditingExpertise(true)} 
                  className="btn btn-edit-expertise"
                >
                  Redaguoti
                </button>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
};

export default PersonalInfo;