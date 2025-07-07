// Get references to DOM elements
const btn = document.getElementById('addBtn');
const input = document.getElementById('textInput');
const status = document.getElementById('status');

// Listen for button clicks
btn.addEventListener('click', async () => {
  // Read and trim the input value
  const text = input.value.trim();

  // Validate: do not allow empty submissions
  if (!text) {
    status.textContent = 'Entry cannot be empty';
    return;
  }

  try {
    // Send POST request to the API endpoint with JSON payload
    const resp = await fetch('/api/add', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text })
    });

    // If server returns an error, display it
    if (!resp.ok) {
      const err = await resp.text();
      status.textContent = 'Error: ' + err;
    } else {
      // On success, notify user and clear input
      status.textContent = 'Entry added!';
      input.value = '';
    }
  } catch (e) {
    // Handle network or other unexpected errors
    status.textContent = 'Network error';
  }
});