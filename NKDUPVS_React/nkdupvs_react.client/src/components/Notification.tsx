import React, { useState } from 'react';
import './Notification.scss';
import UserInfoForm from './UserInfoForm';
import Modal from './Modal';

interface NotificationProps {
  message: string;
  onClose: () => void;
  onSubmit: (data: any) => void;
}

const Notification: React.FC<NotificationProps> = ({ message, onClose, onSubmit }) => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleFillClick = () => {
    setIsModalOpen(true);
  };

  const handleFormSubmit = (data: any) => {
    onSubmit(data);
    setIsModalOpen(false);
  };

  return (
    <>
      <div className="notification">
        <p>{message}</p>
        <button className='fill' onClick={handleFillClick}>PILDYTI</button>
        <button className='refuse' onClick={onClose}>Uždaryti</button>
      </div>
      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)}>
        <UserInfoForm onSubmit={handleFormSubmit} onClose={() => setIsModalOpen(false)} />
      </Modal>
    </>
  );
};

export default Notification;