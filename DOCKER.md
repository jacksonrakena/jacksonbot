# Abyss on Docker
Docker is a powerful containerization and orchestration platform that is heavily recommended to use to run Abyss on.  
Abyss has a bundled Dockerfile that downloads and installs all the requirements it needs.    
  
### Creating a new deployment
Make sure Docker is installed, then:
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
3) Build the Docker image:
```bash
docker build --pull -t abyssalnz/abyss:latest github.com/abyssal/Abyss
```
4) Run the Docker container image, mounting the `abyss-data` directory as a Docker bind mount at `/data`.
```bash
> docker run -d --name abyssconsole --mount type=bind,source="$(pwd)"/abyss-data,target=/data abyssalnz/abyss:latest
```
5) Abyss should now be running!
   
   
### Updating
1) Stop the container and remove the container and image:
```bash
docker stop abyssconsole
docker rm abyssconsole
```
2) Follow steps 3 onwards in the new deployment guide above.