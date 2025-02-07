import React from "react";

const ServiceCard = ({ imgSrc, title, description }) => {
  return (
    <div className="bg-gray-800 rounded-lg shadow-lg transform hover:scale-105 transition-all duration-300 h-96 overflow-hidden">
      {/* Image */}
      <img
        src={imgSrc}
        alt={title}
        className="w-full h-2/3 object-cover"
      />
      
      {/* Content */}
      <div className="p-6">
        <h2 className="text-2xl font-bold mb-2 text-white">{title}</h2>
        <p className="text-gray-300 mb-4">{description}</p>
      </div>
    </div>
  );
};

export default ServiceCard;
