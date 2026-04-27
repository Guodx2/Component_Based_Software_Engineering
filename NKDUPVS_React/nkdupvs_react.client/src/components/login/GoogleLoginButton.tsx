import React from 'react';
import { GoogleOAuthProvider, GoogleLogin } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import { CredentialResponse } from '@react-oauth/google';

const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID || '330440050592-km9oni384gofbl20cru0t449dgontp26.apps.googleusercontent.com';

const GoogleLoginButton = ({ setUser, setShowNotification }: { setUser: React.Dispatch<React.SetStateAction<any>>, setShowNotification: React.Dispatch<React.SetStateAction<boolean>> }) => {
    const navigate = useNavigate();

    /*const onSuccess = async (response: any) => {
        console.log('Login Success:', response);
        const decodedToken: any = jwtDecode(response.credential);

        try {
            const backendResponse = await fetch('/api/auth/google', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ token: token })
            });

            const backendData = await backendResponse.json();
            console.log('Backend login success:', backendData);

            const user = {
                code: backendData.code,
                name: backendData.name || decodedToken.name,
                email: backendData.email || decodedToken.email,
                picture: decodedToken.picture,
                username: backendData.username,
                lastName: backendData.lastName,
                isAdmin: backendData.isAdmin,
                isVerified: backendData.isVerified,
                accessToken: token,
                phoneNumber: backendData.phoneNumber || '',
                isSubmitted: backendData.isSubmitted,
                isMentor: backendData.isMentor, 
                department: backendData.department
            };

            localStorage.removeItem('user');
            localStorage.removeItem('google_token');
            localStorage.setItem('user', JSON.stringify(user));
            localStorage.setItem('google_token', token);

            setUser(user);
            if (!backendData.phoneNumber) {
                setShowNotification(true);
            }
            navigate('/home');
        } catch (error) {
            console.error('Login failed:', error);
        }
    };*/

    const onSuccess = async (credentialResponse: CredentialResponse) => {
        const token = credentialResponse.credential;
        console.log('Login Success:', token);

        if (!token) {
            console.error('No token received from Google login.');
            return;
        }

        const decodedToken: any = jwtDecode(token);

        try {
            const backendResponse = await fetch('http://localhost:5216/api/auth/google', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ token: token })
            });

            const backendData = await backendResponse.json();
            console.log('Backend login success:', backendData);

            const user = {
                code: backendData.code,
                name: backendData.name || decodedToken.name,
                email: backendData.email || decodedToken.email,
                picture: decodedToken.picture,
                username: backendData.username,
                lastName: backendData.lastName,
                isAdmin: backendData.isAdmin,
                isVerified: backendData.isVerified,
                accessToken: token,
                phoneNumber: backendData.phoneNumber || '',
                isSubmitted: backendData.isSubmitted,
                isMentor: backendData.isMentor, 
                department: backendData.department
            };

            localStorage.removeItem('user');
            localStorage.removeItem('google_token');
            localStorage.setItem('user', JSON.stringify(user));
            localStorage.setItem('google_token', token);

            setUser(user);
            if (!backendData.phoneNumber) {
                setShowNotification(true);
            }
            navigate('/home');
        } catch (error) {
            console.error('Login failed:', error);
        }
    };

    const onFailure = () => {
        console.log('Login Failed');
    };

    return (
        <GoogleOAuthProvider clientId={clientId}>
            <div className="google-login-button">
                <GoogleLogin
                    onSuccess={onSuccess}
                    onError={onFailure}
                    type="standard"
                    theme="outline"
                    size="large"
                />
            </div>
        </GoogleOAuthProvider>
    );
};

export default GoogleLoginButton;

/*import React from 'react';
import { GoogleOAuthProvider, GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';

const clientId = '114677274660-0b6upac95821fvl8m12sscgtdbig4dk4.apps.googleusercontent.com';

const GoogleLoginButton = ({
  setUser,
  setShowNotification,
}: {
  setUser: React.Dispatch<React.SetStateAction<any>>;
  setShowNotification: React.Dispatch<React.SetStateAction<boolean>>;
}) => {
  const navigate = useNavigate();

  const onSuccess = async (credentialResponse: CredentialResponse) => {
    const token = credentialResponse.credential;
    if (!token) {
      console.error("No token received");
      return;
    }

    const decodedToken: any = jwtDecode(token);

    try {
      const backendResponse = await fetch('/api/auth/google', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token }),
      });

      const backendData = await backendResponse.json();
      console.log('Backend login success:', backendData);

      const user = {
        code: backendData.code,
        name: backendData.name || decodedToken.name,
        email: backendData.email || decodedToken.email,
        picture: decodedToken.picture,
        username: backendData.username,
        lastName: backendData.lastName,
        isAdmin: backendData.isAdmin,
        isVerified: backendData.isVerified,
        accessToken: token,
        phoneNumber: backendData.phoneNumber || '',
        isSubmitted: backendData.isSubmitted,
        isMentor: backendData.isMentor,
        department: backendData.department,
      };

      localStorage.setItem('user', JSON.stringify(user));
      localStorage.setItem('google_token', token);

      setUser(user);
      if (!backendData.phoneNumber) {
        setShowNotification(true);
      }
      navigate('/home');
    } catch (error) {
      console.error('Login failed:', error);
    }
  };

  const onFailure = () => {
    console.log('Login Failed');
  };

  return (
    <GoogleOAuthProvider clientId={clientId}>
      <div className="google-login-button">
        <GoogleLogin
          onSuccess={onSuccess}
          onError={onFailure}
          type="standard"
          theme="outline"
          size="large"
        />
      </div>
    </GoogleOAuthProvider>
  );
};

export default GoogleLoginButton;*/