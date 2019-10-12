# Abyss on Docker
Docker is a powerful containerization and orchestration platform that is heavily recommended to use to run Abyss on.  
Abyss has a bundled Dockerfile that downloads and installs all the requirements it needs.    
  
### Creating a new deployment
Make sure Docker and Docker Compose is installed, then:
1) Create a directory for Abyss to store it's configuration and data in:
```bash
> mkdir abyss-data
```
2) Navigate to that directory, and store your configuration file:
```bash
> cd abyss-data
> nano abyss.json
> cd ../
```
3) Download Abyss source code:
```bash
> git clone http://github.com/abyssal/Abyss.git
> cd abyss
```
4) Start with Docker Compose:
```bash
> docker-compose up --build -d
```
5) Abyss should now be running!
   
   
### Updating
1) Update the source code:
```bash
> git pull
```
2) Rebuild and recreate with Docker Compose:
```bash
docker-compose up --build -d --force-recreate
```
3) Abyss should now be updated and running!