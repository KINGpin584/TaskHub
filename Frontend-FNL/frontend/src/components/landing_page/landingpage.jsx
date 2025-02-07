import React from 'react';
import Lottie from 'react-lottie';
import taskanimation from '../../assets/taskanimation.json'
function LandingPage() {
  const defaultOptions = {
    loop: true,
    autoplay: true,
    animationData: taskanimation,
    rendererSettings: {
      preserveAspectRatio: 'xMidYMid slice'
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white flex items-center justify-center">
      <div className="flex items-center justify-center w-full">
        {/* Lottie File Container */}
        <div className="w-1/2">
          <Lottie options={defaultOptions} height={400} width={400} />
        </div>

        {/* Text Content */}
        <div className="w-1/2 text-left pl-12">
          <h1 className="text-4xl font-bold mb-4">Manage Your Tasks Effortlessly</h1>
          <p className="text-xl">TaskManager helps you organize your life with simple, intuitive task tracking.</p>
        </div>
      </div>
    </div>
  );
}

export default LandingPage;
