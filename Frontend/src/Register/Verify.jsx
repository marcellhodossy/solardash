import axios from "axios";
import React, {useEffect, useState} from "react";
import { useLocation, useNavigate } from "react-router-dom";

function useQuery() {
    return new URLSearchParams(useLocation().search);
}

function Verify() {
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const query = useQuery();
    const navigate = useNavigate();

    const checkHandler = async () => {
        const token = query.get("token");
        setIsLoading(true);
        setError('');

        try {
            if(token) {
                const response = await axios.post("http://localhost:5030/api/register/verify", {
                    token
                });

                if(response.data.error === 13) {
                    setError("This token is invalid or expired.");
                } else if(response.data.success) {
                    navigate('/');
                }
            } else {
                setError("Token is required");
            }
        } catch (err) {
            setError("Verification failed. Please try again.");
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        checkHandler();
    }, []);

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-md w-full space-y-8">
                <div className="text-center">
                    <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
                        Email Verification
                    </h2>
                </div>
                
                <div className="mt-8 space-y-6">
                    {error && (
                        <div className="rounded-md bg-red-50 p-4">
                            <p className="text-sm text-red-700">{error}</p>
                        </div>
                    )}

                    {isLoading ? (
                        <div className="text-center">
                            <svg className="animate-spin mx-auto h-8 w-8 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                            </svg>
                            <p className="mt-2 text-sm text-gray-600">Verifying your email...</p>
                        </div>
                    ) : (
                        <div className="text-center">
                            <p className="text-sm text-gray-600">
                                {!error && "Email verification complete. You can now login."}
                            </p>
                            <button
                                onClick={() => navigate('/')}
                                className="mt-4 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                            >
                                Go to Login
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default Verify;