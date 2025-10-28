# Use an official Python runtime as a parent image
FROM python:3.11-slim-bookworm

WORKDIR /app

# Instalar el cliente de PostgreSQL
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Copy the requirements file first
COPY requirements.txt .

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the application code into the container
COPY ./app ./app
COPY ./alembic.ini .
COPY ./alembic ./alembic

# Expose the port the app runs on
EXPOSE 8001

# Define the command to run the application
# We will override this with 'command:' in docker-compose to run the migration first
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8001"]