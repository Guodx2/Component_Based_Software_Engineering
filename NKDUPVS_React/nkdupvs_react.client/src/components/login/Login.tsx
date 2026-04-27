import './Login.scss';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import GoogleLoginButton from './GoogleLoginButton';
import React from 'react';

function Login({ setUser, setShowNotification }: { setUser: React.Dispatch<React.SetStateAction<any>>, setShowNotification: React.Dispatch<React.SetStateAction<boolean>> }) {
    const navigate = useNavigate();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [usernameError, setUsernameError] = useState("");
    const [passwordError, setPasswordError] = useState("");

    const handleLogin = async () => {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });
    
            if (!response.ok) {
                const errorData = await response.json();
                setUsernameError(errorData.Message);
                return;
            }
    
            const data = await response.json();
            const user = {
                code: data.UserCode, 
                name: data.Name,
                username: data.Username,
                phoneNumber: data.PhoneNumber,
                isSubmitted: data.isSubmitted // include isSubmitted
            };
    
            localStorage.setItem('user', JSON.stringify(user));
            setUser(user);
            // Check isSubmitted flag instead of phoneNumber.
            if (!data.isSubmitted) {
                setShowNotification(true);
            }
            navigate('/home');
        } catch (error) {
            console.error('Login failed:', error);
            setUsernameError('Login failed. Please try again.');
        }
    };

    return (
        <div className="login-box">
            <h2>Prisijunkite</h2>
            <p className="login-welcome">Sveiki, prisijunkite prie Naujos Kartos Dėstytojų Ugdymo Proceso Valdymo Sistemos</p>
            <GoogleLoginButton setUser={setUser} setShowNotification={setShowNotification} />
        </div>
    );
}

export default Login;