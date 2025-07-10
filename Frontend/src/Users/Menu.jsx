import { Link } from 'react-router-dom';

const MenuPage = () => {
  const menuItems = [
    { path: "/users/map", label: "Map" },
    { path: "/users/editor", label: "Editor" },
  ];

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 p-4">
      <div className="w-full max-w-md bg-white rounded-lg shadow-md p-6">
        <h1 className="text-2xl font-bold text-center mb-6">Menu</h1>
        
        <div className="space-y-3">
          {menuItems.map((item, index) => (
            <Link
              key={index}
              to={item.path}
              className="block w-full text-center py-3 px-4 bg-blue-500 hover:bg-blue-600 text-white rounded-lg transition-colors"
            >
              {item.label}
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
};

export default MenuPage;