import React from 'react';
import { Route, Navigate } from 'react-router-dom';

interface ProtectedRouteProps {
    user: any;
    component: React.FC;
    adminOnly?: boolean;
    nonAdminOnly?: boolean;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ user, component: Component, adminOnly, nonAdminOnly }) => {
  const isAuthenticated = user || localStorage.getItem('user');

  if (!isAuthenticated) {
      return <Navigate to="/" />;
  }
  
  if (adminOnly && !user?.isAdmin) {
      return <Navigate to="/home" />;
  }

  if (nonAdminOnly && user?.isAdmin) {
    return <Navigate to="/home" />;
}
  
  return <Component />;
};

export default ProtectedRoute;