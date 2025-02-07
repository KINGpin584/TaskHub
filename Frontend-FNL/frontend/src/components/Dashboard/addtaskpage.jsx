import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import AddTaskModal from "./addtaskmodal"; // ✅ Ensure this file exists

const AddTaskPage = () => {
  const [showModal, setShowModal] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    if (!showModal) {
      navigate("/dashboard"); // ✅ Redirect back when modal is closed
    }
  }, [showModal, navigate]);

  return (
    <div className="flex items-center justify-center h-screen bg-gray-100">
      {showModal && <AddTaskModal onClose={() => setShowModal(false)} />}
    </div>
  );
};

export default AddTaskPage;
