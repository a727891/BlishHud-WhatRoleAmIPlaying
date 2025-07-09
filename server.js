const express = require('express');
const path = require('path');
const cors = require('cors');

const app = express();
const PORT = 3000;

// Enable CORS for all routes
app.use(cors());

// Serve static files from the public directory
app.use(express.static(path.join(__dirname,'public')));

// Specific route for roles.json
app.get('/roles.json', (req, res) => {
    res.setHeader('Content-Type', 'application/json');
    res.sendFile(path.join(__dirname, 'roles.json'));
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'ok', message: 'What Am I Playing dev server is running' });
});

// Root endpoint
app.get('/', (req, res) => {
    res.json({
        message: 'What Am I Playing Development Server',
        endpoints: {
            roles: '/roles.json',
            health: '/health'
        }
    });
});

app.listen(PORT, () => {
    console.log(`🚀 What Am I Playing dev server running on http://localhost:${PORT}`);
    console.log(`📄 Roles config available at http://localhost:${PORT}/roles.json`);
    console.log(`💚 Health check at http://localhost:${PORT}/health`);
    console.log('');
    console.log('Press Ctrl+C to stop the server');
}); 