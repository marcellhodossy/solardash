import React, { useState, useRef, useEffect } from 'react';

const ImageEditor = () => {
  const [image, setImage] = useState(null);
  const [originalImage, setOriginalImage] = useState(null);
  const canvasRef = useRef(null);
  const fileInputRef = useRef(null);
  
  const [settings, setSettings] = useState({
    brightness: 100,
    contrast: 100,
    saturation: 100,
    sepia: 0,
    hueRotate: 0,
    blur: 0
  });

  // Kép betöltése
  const handleImageUpload = (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      const img = new Image();
      img.onload = () => {
        setOriginalImage(img);
        drawImage(img);
      };
      img.src = event.target.result;
    };
    reader.readAsDataURL(file);
  };

  // Kép rajzolása a canvasra
  const drawImage = (img) => {
    const canvas = canvasRef.current;
    const ctx = canvas.getContext('2d');
    
    canvas.width = img.width;
    canvas.height = img.height;
    
    ctx.filter = getFilterString();
    ctx.drawImage(img, 0, 0);
  };

  // CSS filter string generálása
  const getFilterString = () => {
    return `
      brightness(${settings.brightness}%)
      contrast(${settings.contrast}%)
      saturate(${settings.saturation}%)
      sepia(${settings.sepia}%)
      hue-rotate(${settings.hueRotate}deg)
      blur(${settings.blur}px)
    `;
  };

  // Beállítások változásakor
  useEffect(() => {
    if (originalImage) {
      drawImage(originalImage);
    }
  }, [settings, originalImage]);

  // Kép letöltése
  const handleDownload = () => {
    const canvas = canvasRef.current;
    const dataURL = canvas.toDataURL('image/png');
    const link = document.createElement('a');
    link.download = 'edited-image.png';
    link.href = dataURL;
    link.click();
  };

  // Beállítások visszaállítása
  const resetSettings = () => {
    setSettings({
      brightness: 100,
      contrast: 100,
      saturation: 100,
      sepia: 0,
      hueRotate: 0,
      blur: 0
    });
  };

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-2xl font-bold mb-6">Image Editor</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Feltöltés panel */}
        <div className="bg-white p-4 rounded-lg shadow">
          <h2 className="text-lg font-semibold mb-4">Image upload</h2>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleImageUpload}
            accept="image/*"
            className="hidden"
          />
          <button
            onClick={() => fileInputRef.current.click()}
            className="w-full bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded mb-4"
          >
            Select Image
          </button>
          
          {originalImage && (
            <button
              onClick={handleDownload}
              className="w-full bg-green-500 hover:bg-green-600 text-white py-2 px-4 rounded"
            >
              Download
            </button>
          )}
        </div>

        {/* Canvas */}
        <div className="bg-white p-4 rounded-lg shadow flex justify-center overflow-auto">
          <canvas
            ref={canvasRef}
            className="border border-gray-300 max-w-full"
            style={{ maxHeight: '400px' }}
          />
        </div>

        {/* Szerkesztési beállítások */}
        <div className="bg-white p-4 rounded-lg shadow">
          <h2 className="text-lg font-semibold mb-4">Editor settings</h2>
          
          {Object.entries(settings).map(([key, value]) => (
            <div key={key} className="mb-4">
              <label className="block text-sm font-medium mb-1 capitalize">
                {key.replace(/([A-Z])/g, ' $1')}: {value}
                {key === 'hueRotate' ? '°' : key === 'blur' ? 'px' : '%'}
              </label>
              <input
                type="range"
                min={key === 'hueRotate' ? 0 : key === 'blur' ? 0 : 0}
                max={key === 'hueRotate' ? 360 : key === 'blur' ? 10 : 200}
                value={value}
                onChange={(e) => setSettings({...settings, [key]: parseInt(e.target.value)})}
                className="w-full"
              />
            </div>
          ))}

          <button
            onClick={resetSettings}
            className="w-full bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded mt-4"
          >
            Undo
          </button>
        </div>
      </div>
    </div>
  );
};

export default ImageEditor;