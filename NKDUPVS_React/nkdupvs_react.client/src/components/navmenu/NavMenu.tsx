import { useState, useEffect } from 'react';
import { NavLink, useNavigate } from "react-router-dom";
import './NavMenu.scss';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBars, faArrowRightFromBracket, faBell } from '@fortawesome/free-solid-svg-icons';
import { gapi } from 'gapi-script';
import NotificationModal from '../NotificationModal';

interface NavMenuProps {
    user: any;
    setUser: React.Dispatch<React.SetStateAction<any>>;
    setShowNotification: React.Dispatch<React.SetStateAction<boolean>>;
}

const NavMenu: React.FC<NavMenuProps> = ({ user, setUser, setShowNotification }) => {
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [isMobile, setIsMobile] = useState(window.innerWidth < 769);
    const navigate = useNavigate();

    // State for accepted requests count (badge)
    const [acceptedRequestsCount, setAcceptedRequestsCount] = useState<number>(0);
    // New state for notifications modal
    const [notifications, setNotifications] = useState<any[]>([]);
    const [showNotifications, setShowNotifications] = useState<boolean>(false);

    useEffect(() => {
        const handleResize = () => {
            setIsMobile(window.innerWidth < 769);
        };
        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

    const handleLogout = () => {
        localStorage.removeItem('user');
        localStorage.removeItem('google_token');
        setUser(null);
        setShowNotification(false);
        navigate("/");
    
        const authInstance = gapi?.auth2?.getAuthInstance?.();
        if (authInstance) {
            authInstance.signOut().then(() => {
                authInstance.disconnect();
            });
        }
    };

    const toggleMenu = () => {
        setIsMenuOpen(true);
    };

    const closeMenu = () => {
        setIsMenuOpen(false);
    };

    // Fetch accepted notification count (badge)
    useEffect(() => {
        if (user && !user.isMentor && user.isVerified) {
            Promise.all([
                fetch(`http://localhost:5216/api/mentee/acceptedRequestsCount/${user.code}`).then(res => res.json()),
                fetch(`http://localhost:5216/api/mentee/rejectedRequestsCount/${user.code}`).then(res => res.json())
            ])
            .then(([acceptedCount, rejectedCount]: [number, number]) => {
                setAcceptedRequestsCount(acceptedCount + rejectedCount);
            })
            .catch(err => console.error('Error fetching combined notifications count:', err));
        }
    }, [user]);

    // Function to load notifications and mark them read
    const openNotifications = async () => {
        if (user && !user.isMentor && user.isVerified) {
            try {
                // Fetch both accepted and rejected notifications in parallel
                const [acceptedRes, rejectedRes] = await Promise.all([
                    fetch(`http://localhost:5216/api/mentee/acceptedRequests/${user.code}`),
                    fetch(`http://localhost:5216/api/mentee/rejectedRequests/${user.code}`)
                ]);
                const accepted = await acceptedRes.json();
                const rejected = await rejectedRes.json();
                // Combine notifications
                const combined = accepted.concat(rejected).sort((a: { requestDate: string }, b: { requestDate: string }) => new Date(b.requestDate).getTime() - new Date(a.requestDate).getTime());
                setNotifications(combined);
                setShowNotifications(true);
                // Mark both types as read
                await Promise.all([
                    fetch(`http://localhost:5216/api/mentee/markNotificationsRead/${user.code}`, { method: 'POST' }),
                    fetch(`https://localhost:7124/api/mentee/markRejectedNotificationsRead/${user.code}`, { method: 'POST' })
                ]);
                // Reset badge count (if you have separate counts, combine them as needed)
                setAcceptedRequestsCount(0);
            } catch (err) {
                console.error(err);
            }
        }
    };

    const navLinks = (
        <>
            <NavLink to="/timetable" className="nav-link" onClick={closeMenu}>Tvarkaraštis</NavLink>
            {user && !user.isMentor && !user.isAdmin && user.isVerified && (
                <NavLink to="/semesterplan" className="nav-link" onClick={closeMenu}>
                    Semestro planas
                </NavLink>
            )}
            {user && user.isMentor && user.isVerified && (
                <NavLink to="/mymentees" className="nav-link" onClick={closeMenu}>Mano ugdytiniai</NavLink>
            )}
            {user && user.isMentor && (
                <NavLink to="/tasks" className="nav-link" onClick={closeMenu}>Užduotys</NavLink>
            )}
            <NavLink to="/events" className="nav-link" onClick={closeMenu}>Įvykiai</NavLink>
            {user && user.isAdmin && (
                <div>
                    <NavLink to="/accounts" className="nav-link" onClick={closeMenu}>Paskyros</NavLink>
                </div>
            )}
            {isMobile && user && (
                <>
                    <div className="nav-link personal-info" onClick={() => { closeMenu(); navigate('/personalinfo'); }}>
                        {user.code + " " + user.name + " " + user.lastName}
                    </div>
                    <div className="nav-link logout" onClick={() => { closeMenu(); handleLogout(); }}>
                        atsijungti <FontAwesomeIcon icon={faArrowRightFromBracket} />
                    </div>
                </>
            )}
        </>
    );

    return (
        <>
            {isMenuOpen && <div className="menu-backdrop" onClick={closeMenu} />}
            <nav className="navbar">
                <div className="navbar-header">
                    {!isMenuOpen && isMobile && ( 
                        <button className="hamburger-menu" onClick={toggleMenu}>
                            <FontAwesomeIcon icon={faBars} />
                        </button>
                    )}
                </div>
                <div className="navbar-content">
                    <div className={`nav-left ${isMenuOpen ? 'open' : ''}`}>
                        {navLinks}
                    </div>
                    {!isMobile && user && (
                        <div className="nav-right">
                            <p className="personal-info mb-0" onClick={() => { closeMenu(); navigate('/personalinfo'); }}>
                                {user.code + " " + user.name + " " + user.lastName}
                            </p>
                            {/* Bell icon placed to the right of personal info */}
                            {user && !user.isMentor && user.isVerified && (
                                <div style={{ position: 'relative', marginLeft: '1rem', cursor: 'pointer' }} title="Pranešimai" onClick={openNotifications}>
                                    <FontAwesomeIcon icon={faBell} style={{ fontSize: '1.2rem' }} />
                                    {acceptedRequestsCount > 0 && (
                                        <span style={{
                                            position: 'absolute',
                                            top: '-5px',
                                            right: '-5px',
                                            background: 'red',
                                            color: 'white',
                                            fontSize: '0.75rem',
                                            borderRadius: '50%',
                                            width: '18px',
                                            height: '18px',
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center'
                                        }}>
                                            {acceptedRequestsCount}
                                        </span>
                                    )}
                                </div>
                            )}
                            <a className="logout" href="#" onClick={handleLogout}>
                                atsijungti <FontAwesomeIcon icon={faArrowRightFromBracket} />
                            </a>
                        </div>
                    )}
                </div>
            </nav>
            {showNotifications && (
                <NotificationModal 
                    notifications={notifications}
                    onClose={() => setShowNotifications(false)}
                />
            )}
        </>
    );
};

export default NavMenu;