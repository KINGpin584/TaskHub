import React from "react";
import { Routes, Route } from "react-router-dom";
import Login from "./components/login/login";
import Signup from "./components/signup/signup";
import Dashboard from "./components/Dashboard/dashboard";
import TaskHub from "./components/Dashboard/taskhub";
import MyTasks from "./components/Dashboard/mytask";
import AddTaskPage from "./components/Dashboard/addtaskpage";
import PrivateRoute from "./routes/privateroute";
import TaskManagerFeatures from "./components/Features/features";
import LandingPage from "./components/landing_page/landingpage";
import Navbar from "./components/Navbar/navbar";

function App() {
  return (
    <div className="flex flex-col min-h-screen">
      {/* Navbar will appear on all pages */}
      <Navbar />
      
      {/* Main content area */}
      <main className="flex-grow">
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<Login />} />
          <Route path="/signup" element={<Signup />} />
          <Route path="/features" element={<TaskManagerFeatures />} />

          {/* Protected routes */}
          <Route element={<PrivateRoute />}>
            {/* Dashboard acts as a layout route */}
            <Route path="/dashboard" element={<Dashboard />}>
              {/* Nested routes are rendered inside Dashboard's <Outlet /> */}
              <Route index element={<TaskHub />} />
              <Route path="my-tasks" element={<MyTasks />} />
              <Route path="add-task" element={<AddTaskPage />} />
            </Route>
          </Route>
        </Routes>
      </main>
    </div>
  );
}

export default App;