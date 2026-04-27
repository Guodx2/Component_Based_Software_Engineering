import React, { useState, useEffect } from 'react';
import './MentorExpertiseForm.scss';

interface ExpertiseItem {
  id?: number;
  studyProgram: number;
  specialization?: number;
  priority: number;
}

interface MentorExpertiseFormProps {
  mentorCode: string;
  onSuccess: () => void;
}

// New state types for fetched data.
interface StudyProgramItem {
  value: number;
  label: string;
}

interface SpecializationItem {
  value: number;
  label: string;
  studyProgram: number;
}

const MentorExpertiseForm: React.FC<MentorExpertiseFormProps> = ({ mentorCode, onSuccess }) => {
  const [expertiseList, setExpertiseList] = useState<ExpertiseItem[]>([]);
  const [studyPrograms, setStudyPrograms] = useState<StudyProgramItem[]>([]);
  const [specializations, setSpecializations] = useState<SpecializationItem[]>([]);
  const [specializationsMapping, setSpecializationsMapping] = useState<{ [key: number]: SpecializationItem[] }>({});
  const [acceptingNewMentees, setAcceptingNewMentees] = useState<boolean>(false);

  // Fetch mentor details to get the current value of acceptingNewMentees.
  useEffect(() => {
    fetch(`http://localhost:5216/api/mentor/${mentorCode}`)
      .then(res => res.json())
      .then((data) => {
        // Assume returning JSON has an "acceptingNewMentees" boolean field.
        setAcceptingNewMentees(data.acceptingNewMentees);
      })
      .catch(err => console.error('Error fetching mentor details:', err));
  }, [mentorCode]);

  // Fetch study programs and specializations from the database.
  useEffect(() => {
    // Fetch study programs
    fetch('http://localhost:5216/api/studyprograms')
      .then(res => res.json())
      .then((data: { value: string; label: string }[]) => {
        const programs = data.map(sp => ({ value: parseInt(sp.value), label: sp.label }));
        setStudyPrograms(programs);
      })
      .catch(err => console.error('Error fetching study programs:', err));

    // Fetch specializations
    fetch('http://localhost:5216/api/specializations')
      .then(res => res.json())
      .then((data: { value: string; label: string; fk_id_StudyPrograms: string }[]) => {
        const specs = data.map(s => ({
          value: parseInt(s.value),
          label: s.label,
          studyProgram: parseInt(s.fk_id_StudyPrograms)
        }));
        setSpecializations(specs);
        // Build mapping: for each studyProgram, group its specializations.
        const mapping: { [key: number]: SpecializationItem[] } = specs.reduce((acc, spec) => {
          if (!acc[spec.studyProgram]) {
            acc[spec.studyProgram] = [];
          }
          acc[spec.studyProgram].push(spec);
          return acc;
        }, {} as { [key: number]: SpecializationItem[] });
        setSpecializationsMapping(mapping);
      })
      .catch(err => console.error('Error fetching specializations:', err));
  }, []);

  // Fetch existing expertise for editing
  useEffect(() => {
    fetch(`http://localhost:5216/api/mentor/expertise/${mentorCode}`)
      .then(res => res.json())
      .then((data: ExpertiseItem[]) => {
        if (data && data.length > 0) {
          const updatedData = data.map(item => {
            // If specialization is null and there is a mapping for the chosen study program, set a default.
            if (item.specialization == null && specializationsMapping[item.studyProgram] && specializationsMapping[item.studyProgram].length > 0) {
              return { ...item, specialization: specializationsMapping[item.studyProgram][0].value };
            }
            return item;
          });
          setExpertiseList(updatedData.sort((a, b) => a.priority - b.priority));
        }
      })
      .catch(err => console.error('Error fetching expertise:', err));
  }, [mentorCode, specializationsMapping]);

  const addExpertiseItem = () => {
    // Use the first study program from fetched data.
    const defaultStudyProgram = studyPrograms.length > 0 ? studyPrograms[0].value : 0;
    const defaultSpecialization = defaultStudyProgram && specializationsMapping[defaultStudyProgram] && specializationsMapping[defaultStudyProgram].length > 0
      ? specializationsMapping[defaultStudyProgram][0].value
      : undefined;
    setExpertiseList([
      ...expertiseList,
      { studyProgram: defaultStudyProgram, specialization: defaultSpecialization, priority: expertiseList.length + 1 }
    ]);
  };

  const updateExpertiseItem = (index: number, field: keyof ExpertiseItem, value: any) => {
    const newList = [...expertiseList];
    // If studyProgram changes, update specialization default if not set.
    if (field === 'studyProgram') {
      const newStudyProgram = parseInt(value, 10);
      newList[index][field] = newStudyProgram;
      if (specializationsMapping[newStudyProgram] && specializationsMapping[newStudyProgram].length > 0) {
        newList[index].specialization = specializationsMapping[newStudyProgram][0].value;
      } else {
        newList[index].specialization = undefined;
      }
    } else {
      newList[index] = { ...newList[index], [field]: value };
    }
    setExpertiseList(newList);
  };

  const removeExpertiseItem = (index: number) => {
    setExpertiseList(expertiseList.filter((_, i) => i !== index));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      mentorCode,
      acceptingNewMentees, // include the new setting in the payload
      ExpertiseList: expertiseList
    };

    fetch('http://localhost:5216/api/mentor/updateExpertise', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
      .then(res => {
        if (res.ok) {
          window.location.reload();
        }
      })
      .catch(err => console.error(err));
  };

  return (
    <form className="mentor-expertise-form" onSubmit={handleSubmit}>
      <h3>Redaguoti studijų programų ir specializacijų prioritetus</h3>
      <div className="form-group">
        <label>
          <input
            type="checkbox"
            checked={acceptingNewMentees}
            onChange={(e) => setAcceptingNewMentees(e.target.checked)}
          />
          Priimu naujų ugdytinių
        </label>
      </div>
      {expertiseList.map((item, index) => (
        <div key={index} className="expertise-item">
          <div className="form-group">
            <label>Prioritetas:</label>
            <input
              type="number"
              value={item.priority}
              onChange={(e) => updateExpertiseItem(index, 'priority', parseInt(e.target.value, 10))}
              min={1}
              required
            />
          </div>
          <div className="form-group">
            <label>Studijų programa:</label>
            <select
              value={item.studyProgram}
              onChange={(e) => updateExpertiseItem(index, 'studyProgram', parseInt(e.target.value, 10))}
              required
            >
              {studyPrograms.map(sp => (
                <option key={sp.value} value={sp.value}>{sp.label}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label>Specializacija:</label>
            {specializationsMapping[item.studyProgram] ? (
              specializationsMapping[item.studyProgram].length > 0 ? (
                <select
                  value={item.specialization !== undefined ? item.specialization : specializationsMapping[item.studyProgram][0].value}
                  onChange={(e) => updateExpertiseItem(index, 'specialization', parseInt(e.target.value, 10))}
                  required
                >
                  {specializationsMapping[item.studyProgram].map(spec => (
                    <option key={spec.value} value={spec.value}>
                      {spec.label}
                    </option>
                  ))}
                </select>
              ) : (
                <select disabled>
                  <option>Nėra specializacijų</option>
                </select>
              )
            ) : (
              <select disabled>
                <option>Nėra specializacijų</option>
              </select>
            )}
          </div>
          <div className="btn-container">
            <button type="button" className="btn btn-remove" onClick={() => removeExpertiseItem(index)}>
              Šalinti
            </button>
          </div>
        </div>
      ))}
      <div className="form-footer">
        <button type="button" className="btn btn-add" onClick={addExpertiseItem}>
          Pridėti
        </button>
        <button type="submit" className="btn btn-submit">
          Išsaugoti
        </button>
      </div>
    </form>
  );
};

export default MentorExpertiseForm;