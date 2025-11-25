FROM python:3.10-slim-bullseye  

WORKDIR /app

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY ./app ./app

EXPOSE 8001

CMD ["uvicorn", "app.main:app", "--host", "::", "--port", "8001"]
