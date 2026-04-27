import React, { useState, useEffect } from 'react';
import './UserInfoForm.scss';

interface UserInfoFormProps {
  onSubmit: (data: any) => void;
  onClose: () => void;
}

const UserInfoForm: React.FC<UserInfoFormProps> = ({ onSubmit, onClose }) => {
  const [userType, setUserType] = useState('');
  const [selectedStudyProgram, setSelectedStudyProgram] = useState('');
  const [selectedSpecialization, setSelectedSpecialization] = useState('');
  const [selectedDepartment, setSelectedDepartment] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [errors, setErrors] = useState({ phoneNumber: '' });

  // Fetched data states
  const [studyPrograms, setStudyPrograms] = useState<{ value: string; label: string }[]>([]);
  const [departments, setDepartments] = useState<{ value: string; label: string }[]>([]);
  const [specializations, setSpecializations] = useState<{ value: string; label: string; fk_id_StudyPrograms: string }[]>([]);
  // Build mapping keyed by studyProgramId (as a string)
  const [specializationsMapping, setSpecializationsMapping] = useState<{ [key: string]: { value: string; label: string }[] }>({});

  // Fetch study programs from API
  useEffect(() => {
    fetch('http://localhost:5216/api/studyprograms')
      .then(res => res.json())
      .then((data: { value: string; label: string }[]) => {
        setStudyPrograms(data);
      })
      .catch(err => console.error("Error fetching study programs:", err));
  }, []);

  // Fetch departments from API
  useEffect(() => {
    fetch('http://localhost:5216/api/departments')
      .then(res => res.json())
      .then((data: { value: string; label: string }[]) => {
        setDepartments(data);
      })
      .catch(err => console.error("Error fetching departments:", err));
  }, []);

  // Fetch specializations from API and build mapping by studyProgramId
  useEffect(() => {
    fetch('http://localhost:5216/api/specializations')
      .then(res => res.json())
      .then((data: { value: string; label: string; fk_id_StudyPrograms: string }[]) => {
        setSpecializations(data);
        const mapping = data.reduce((acc: { [key: string]: { value: string; label: string }[] }, spec) => {
          if (!acc[spec.fk_id_StudyPrograms]) {
            acc[spec.fk_id_StudyPrograms] = [];
          }
          acc[spec.fk_id_StudyPrograms].push({ value: spec.value, label: spec.label });
          return acc;
        }, {});
        setSpecializationsMapping(mapping);
      })
      .catch(err => console.error("Error fetching specializations:", err));
  }, []);

  const handleStudyProgramChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setSelectedStudyProgram(value);
    if (specializationsMapping[value] && specializationsMapping[value].length > 0) {
      // Set the first available specialization as the default
      setSelectedSpecialization(specializationsMapping[value][0].value);
    } else {
      setSelectedSpecialization('');
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Validate phone number format (+3706XXXXXXX)
    const phonePattern = /^\+3706\d{7}$/;
    if (!phonePattern.test(phoneNumber)) {
      setErrors({ phoneNumber: 'Neteisingas tel. nr. formatas. Pavyzdys: +37061234567' });
      return;
    } else {
      setErrors({ phoneNumber: '' });
    }

    const data: any = {
      userType,
      phoneNumber,
    };

    if (userType === 'mentee') {
      data.studyProgram = parseInt(selectedStudyProgram, 10);
      if (specializationsMapping[selectedStudyProgram]) {
        data.specialization = parseInt(selectedSpecialization, 10);
      }
    }

    if (userType === 'mentor') {
      data.department = parseInt(selectedDepartment, 10);
    }

    onSubmit(data);
  };

  return (
    <form onSubmit={handleSubmit} className="user-info-form">
      <button type="button" className="close-button" onClick={onClose}>
        &times;
      </button>
      <div className="form-group">
        <h2>Asmeniniai duomenys</h2>
        <label htmlFor="userType">Naudotojo tipas:</label>
        <select
          id="userType"
          value={userType}
          onChange={(e) => {
            setUserType(e.target.value);
            setSelectedStudyProgram('');
            setSelectedSpecialization('');
            setSelectedDepartment('');
            setPhoneNumber('');
          }}
          required
        >
          <option value="">Pasirinkite</option>
          <option value="mentor">Mentorius</option>
          <option value="mentee">Ugdytinis</option>
        </select>
      </div>

      {userType === 'mentee' && (
        <>
          <div className="form-group">
            <label htmlFor="studyProgram">Studijų programa:</label>
            <select
              id="studyProgram"
              value={selectedStudyProgram}
              onChange={handleStudyProgramChange}
              required
            >
              <option value="">Pasirinkite studijų programą</option>
              {studyPrograms.map((sp) => (
                <option key={sp.value} value={sp.value}>
                  {sp.label}
                </option>
              ))}
            </select>
          </div>
          {selectedStudyProgram &&
            specializationsMapping[selectedStudyProgram] &&
            specializationsMapping[selectedStudyProgram].length > 0 && (
              <div className="form-group">
                <label htmlFor="specialization">Specializacija:</label>
                <select
                  id="specialization"
                  value={selectedSpecialization}
                  onChange={(e) => setSelectedSpecialization(e.target.value)}
                  required
                >
                  <option value="">Pasirinkite specializaciją</option>
                  {specializationsMapping[selectedStudyProgram].map((spec) => (
                    <option key={spec.value} value={spec.value}>
                      {spec.label}
                    </option>
                  ))}
                </select>
              </div>
            )}
        </>
      )}

      {userType === 'mentor' && (
        <div className="form-group">
          <label htmlFor="department">Katedra:</label>
          <select
            id="department"
            value={selectedDepartment}
            onChange={(e) => setSelectedDepartment(e.target.value)}
            required
          >
            <option value="">Pasirinkite katedrą</option>
            {departments.map((dep) => (
              <option key={dep.value} value={dep.value}>
                {dep.label}
              </option>
            ))}
          </select>
        </div>
      )}

      <div className="form-group">
        <label htmlFor="phoneNumber">Telefono numeris:</label>
        <input
          type="text"
          id="phoneNumber"
          value={phoneNumber}
          onChange={(e) => setPhoneNumber(e.target.value)}
          placeholder="+3706XXXXXXX"
          required
        />
        {errors.phoneNumber && <span className="error">{errors.phoneNumber}</span>}
      </div>

      <button type="submit" className="submit-button">
        Pateikti
      </button>
    </form>
  );
};

export default UserInfoForm;