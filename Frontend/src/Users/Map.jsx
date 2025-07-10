import axios from 'axios';
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  useMap
} from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';


const FitBounds = ({ positions }) => {
  const map = useMap();

  React.useEffect(() => {
    const valid = positions
      .map(p => [parseFloat(p.lat), parseFloat(p.lng)])
      .filter(([lat, lng]) => !isNaN(lat) && !isNaN(lng));

    if (valid.length > 0) {
      map.fitBounds(valid, { padding: [50, 50] });
    }
  }, [positions, map]);

  return null;
};

function Map() {
  const [lng, setLng] = useState('');
  const [lat, setLat] = useState('');
  const [name, setName] = useState('');
  const [id, setId] = useState(null);
  const [positions, setPositions] = useState([]);
  const [isEditing, setIsEditing] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/');
      return;
    }

    fetchPositions();
  }, [navigate]);

  const fetchPositions = async () => {
    const token = localStorage.getItem('token');
    try {
      const res = await axios.get("http://localhost:5030/api/users/solargraph", {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      setPositions(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/');
      return;
    }

    try {
      const action = isEditing ? "Update" : "Create";
      const url = "http://localhost:5030/api/users/solargraph";
      
      const payload = {
        lng,
        lat,
        name,
        action,
        ...(isEditing && { id })
      };

      await axios.post(url, payload, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      await fetchPositions();
      resetForm();
    } catch (err) {
      console.error(err);
    }
  };

  const handleDelete = async (id) => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/');
      return;
    }

    if (window.confirm('Are you sure you want to delete this location?')) {
      try {
        await axios.post(
          "http://localhost:5030/api/users/solargraph",
          { id, action: "Delete" },
          {
            headers: {
              Authorization: `Bearer ${token}`
            }
          }
        );
        await fetchPositions();
        resetForm();
      } catch (err) {
        console.error(err);
      }
    }
  };

  const handleEdit = (position) => {
    setId(position.id);
    setLng(position.lng);
    setLat(position.lat);
    setName(position.name);
    setIsEditing(true);
  };

  const resetForm = () => {
    setId(null);
    setLng('');
    setLat('');
    setName('');
    setIsEditing(false);
  };

  const getCurrentPosition = () => {
    if (!navigator.geolocation) {
      alert('Your browser does not support geolocation.');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLat(position.coords.latitude.toString());
        setLng(position.coords.longitude.toString());
      },
      (error) => {
        alert('Failed to get current position.');
        console.error(error);
      }
    );
  };

  return (
    <div className="flex h-screen">
      {/* Left side - form and list */}
      <div className="w-1/3 p-4 bg-gray-50 border-r border-gray-200 overflow-y-auto">
        <h2 className="text-2xl font-bold mb-6 text-gray-800">
          {isEditing ? 'Edit Position' : 'Add New Position'}
        </h2>
        
        <form onSubmit={handleSubmit} className="space-y-4 mb-8">
          <button 
            type="button" 
            onClick={getCurrentPosition} 
            className="w-full bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded transition duration-200"
          >
            Get Current Position
          </button>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Longitude (lng):</label>
            <input
              type="text"
              value={lng}
              onChange={e => setLng(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Latitude (lat):</label>
            <input
              type="text"
              value={lat}
              onChange={e => setLat(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Name:</label>
            <input
              type="text"
              value={name}
              onChange={e => setName(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div className="flex space-x-2">
            <button 
              type="submit" 
              className="flex-1 bg-green-500 hover:bg-green-600 text-white py-2 px-4 rounded transition duration-200"
            >
              {isEditing ? 'Update' : 'Add'} Position
            </button>
            
            {isEditing && (
              <button
                type="button"
                onClick={resetForm}
                className="flex-1 bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded transition duration-200"
              >
                Cancel
              </button>
            )}
          </div>
        </form>

        <h3 className="text-xl font-semibold mb-4 text-gray-800">Saved Locations</h3>
        <div className="space-y-2">
          {positions.map((position) => (
            <div key={position.id} className="p-3 border border-gray-200 rounded-md hover:bg-gray-100 transition duration-150">
              <div className="flex justify-between items-start">
                <div>
                  <h4 className="font-medium text-gray-900">{position.name}</h4>
                  <p className="text-sm text-gray-600">
                    Lat: {position.lat}, Lng: {position.lng}
                  </p>
                </div>
                <div className="flex space-x-2">
                  <button
                    onClick={() => handleEdit(position)}
                    className="text-blue-500 hover:text-blue-700 text-sm font-medium"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(position.id)}
                    className="text-red-500 hover:text-red-700 text-sm font-medium"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Right side - map */}
      <div className="flex-1">
        <MapContainer
          className="h-full w-full"
          zoom={6}
          center={[47.4979, 19.0402]}
        >
          <TileLayer
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          />

          {positions.map((pos) => {
            const latNum = parseFloat(pos.lat);
            const lngNum = parseFloat(pos.lng);

            if (isNaN(latNum) || isNaN(lngNum)) return null;

            return (
              <Marker key={pos.id} position={[latNum, lngNum]}>
                <Popup>
                  <div className="space-y-1">
                    <h3 className="font-semibold">{pos.name}</h3>
                    <p>Lat: {pos.lat}</p>
                    <p>Lng: {pos.lng}</p>
                    <div className="flex space-x-2 mt-1">
                      <button
                        onClick={() => {
                          handleEdit(pos);
                          document.querySelector('.leaflet-popup-close-button').click();
                        }}
                        className="text-blue-500 hover:text-blue-700 text-sm"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => {
                          handleDelete(pos.id);
                          document.querySelector('.leaflet-popup-close-button').click();
                        }}
                        className="text-red-500 hover:text-red-700 text-sm"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </Popup>
              </Marker>
            );
          })}

          <FitBounds positions={positions} />
        </MapContainer>
      </div>
    </div>
  );
}

export default Map;