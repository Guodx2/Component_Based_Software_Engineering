import React, { useState, useEffect } from 'react';
import './MentorExpertiseDisplay.scss';

interface MentorExpertise {
    id: number;
    mentorCode: string;
    studyProgram: number;
    specialization?: number | null;
    priority: number;
}

interface MappingItem {
    value: string;
    label: string;
}

interface Props {
    mentorCode: string;
}

const MentorExpertiseDisplay: React.FC<Props> = ({ mentorCode }) => {
    const [expertiseList, setExpertiseList] = useState<MentorExpertise[]>([]);
    const [loading, setLoading] = useState(true);
    const [studyProgramsMapping, setStudyProgramsMapping] = useState<{ [key: number]: string }>({});
    const [specializationsMapping, setSpecializationsMapping] = useState<{ [key: number]: string }>({});

    // Fetch the mappings from the database
    useEffect(() => {
        Promise.all([
            fetch('http://localhost:5216/api/studyprograms').then(res => res.json()),
            fetch('http://localhost:5216/api/specializations').then(res => res.json())
        ])
            .then(([spData, specData]) => {
                const spMap: { [key: number]: string } = {};
                spData.forEach((item: MappingItem) => {
                    spMap[parseInt(item.value)] = item.label;
                });
                const specMap: { [key: number]: string } = {};
                specData.forEach((item: MappingItem) => {
                    specMap[parseInt(item.value)] = item.label;
                });
                setStudyProgramsMapping(spMap);
                setSpecializationsMapping(specMap);
            })
            .catch(err => {
                console.error('Failed to fetch study program/specialization mappings:', err);
            });
    }, []);

    // Fetch mentor expertise list
    useEffect(() => {
        fetch(`http://localhost:5216/api/mentor/expertise/${mentorCode}`)
            .then(res => res.json())
            .then((data: MentorExpertise[]) => {
                // Ensure the list is sorted by priority
                setExpertiseList(data.sort((a, b) => a.priority - b.priority));
                setLoading(false);
            })
            .catch(err => {
                console.error('Error fetching mentor expertise:', err);
                setLoading(false);
            });
    }, [mentorCode]);

    if (loading) return <p>Kraunama...</p>;
    if (expertiseList.length === 0) return <p>Nėra pridėtų studijų sričių prioriteto.</p>;

    return (
        <div className="mentor-expertise-display">
            <h3>Mano studijų sričių pasirinkimai</h3>
            <ul className="mentor-expertise-list">
                {expertiseList.map(item => (
                    <li key={item.id} className="mentor-expertise-item">
                        <div className="mentor-expertise-details">
                            <span className="expertise-label">Prioritetas: {item.priority}</span>
                            <span>
                                {studyProgramsMapping[item.studyProgram] || `Program ${item.studyProgram}`}
                            </span>
                            {item.specialization && (
                                <span>
                                    Specializacija: {specializationsMapping[item.specialization] || `Specialization ${item.specialization}`}
                                </span>
                            )}
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default MentorExpertiseDisplay;