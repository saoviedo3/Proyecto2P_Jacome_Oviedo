import React, { useState, useEffect } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import "bootstrap/dist/css/bootstrap.min.css";

const App = () => {
    const [connection, setConnection] = useState(null);
    const [messages, setMessages] = useState([]);
    const [user, setUser] = useState(null);
    const [message, setMessage] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [isRegistering, setIsRegistering] = useState(false);

    useEffect(() => {
        fetch("http://localhost:5134/api/auth/currentUser", { credentials: "include" })
            .then(response => response.json())
            .then(data => {
                if (data.username) {
                    setUser(data.username);
                }
            })
            .catch(() => setUser(null));
    }, []);

    useEffect(() => {
        if (!user) return; 

        const connect = new HubConnectionBuilder()
            .withUrl("http://localhost:5134/chatHub", { withCredentials: true })
            .withAutomaticReconnect()
            .build();

        connect.start()
            .then(() => {
                console.log("✅ Conectado a SignalR");
                setConnection(connect);

                connect.off("ReceiveMessage");

                connect.on("ReceiveMessage", (user, message) => {
                    setMessages(prevMessages => [...prevMessages, { user, message }]);
                });
            })
            .catch(err => console.error("❌ Error al conectar:", err));

        return () => {
            connect.stop(); 
            console.log("🔴 Conexión cerrada");
        };
    }, [user]); // 🔄 Solo se ejecuta cuando `user` cambia

    const sendMessage = async () => {
        if (connection && message && user) {
            try {
                await connection.invoke("SendMessage", user, message); // <-- Ahora incluye el usuario
                setMessage("");
            } catch (err) {
                console.error("Error al enviar el mensaje: ", err);
            }
        } else {
            console.error("❌ No estás autenticado.");
        }
    };

    const handleAuth = async (endpoint) => {
        const response = await fetch(`http://localhost:5134/api/auth/${endpoint}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password }),
            credentials: "include"
        });

        if (response.ok) {
            setUser(username);
        } else {
            alert("Error en la autenticación. Intenta nuevamente.");
        }
    };

    const handleLogout = async () => {
        await fetch("http://localhost:5134/api/auth/logout", { method: "POST", credentials: "include" });
        setUser(null);
        setMessages([]); 
    };

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-6">
                    <div className="card shadow-lg p-4">
                        <h2 className="text-center text-primary mb-4">Chat en Tiempo Real</h2>

                        {user ? (
                            <>
                                <div className="d-flex justify-content-between align-items-center mb-3">
                                    <p className="fw-bold text-success">Bienvenido, {user} 👋</p>
                                    <button onClick={handleLogout} className="btn btn-danger">Cerrar Sesión</button>
                                </div>

                                <div className="input-group mb-3">
                                    <input
                                        type="text"
                                        placeholder="Escribe un mensaje..."
                                        className="form-control"
                                        value={message}
                                        onChange={e => setMessage(e.target.value)}
                                    />
                                    <button className="btn btn-primary" onClick={sendMessage}>Enviar</button>
                                </div>
                            </>
                        ) : (
                            <div>
                                <h4 className="text-center">{isRegistering ? "Registro" : "Inicio de Sesión"}</h4>
                                <input
                                    type="text"
                                    placeholder="Usuario"
                                    className="form-control mb-2"
                                    value={username}
                                    onChange={e => setUsername(e.target.value)}
                                />
                                <input
                                    type="password"
                                    placeholder="Contraseña"
                                    className="form-control mb-2"
                                    value={password}
                                    onChange={e => setPassword(e.target.value)}
                                />
                                <button
                                    className="btn btn-success w-100 mb-2"
                                    onClick={() => handleAuth(isRegistering ? "register" : "login")}
                                >
                                    {isRegistering ? "Registrarse" : "Iniciar Sesión"}
                                </button>
                                <button className="btn btn-link w-100" onClick={() => setIsRegistering(!isRegistering)}>
                                    {isRegistering ? "¿Ya tienes cuenta? Inicia sesión" : "¿No tienes cuenta? Regístrate"}
                                </button>
                            </div>
                        )}
                    </div>

                    {user && (
                        <div className="card mt-4 shadow-lg">
                            <div className="card-header bg-primary text-white">📩 Mensajes</div>
                            <div className="card-body" style={{ maxHeight: "300px", overflowY: "auto" }}>
                                <ul className="list-group">
                                    {messages.map((msg, index) => (
                                        <li key={index} className="list-group-item">
                                            <strong>{msg.user}:</strong> {msg.message}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default App;
