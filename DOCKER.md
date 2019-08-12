# Abyss on Docker
Docker is a powerful containerization and orchestration platform that is heavily recommended to use to run Abyss on.  
Abyss has a bundled Dockerfile that downloads and installs all the requirements it needs.  
  
There are Bash scripts that I have written that automate most of this stuff, which are available at [abyssal512/abyss-scripts](http://github.com/abyssal512/abyss-scripts).

### Creating a new deployment
You can use my [bash script for initializing deployments](https://github.com/abyssal512/abyss-scripts/blob/master/new-abyss-deployment) to do all of this. Otherwise:  
0) Install Docker.
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
3) Build the Docker container image:
```bash
> docker build http://github.com/abyssal512/abyss.git -t abyss
Successfully tagged abyss:latest
```
4) Run the Docker container image, mounting the `abyss-data` directory as a Docker bind mount at `/data`
```bash
> docker run -d --name abyssconsole --mount type=bind,source="$(pwd)"/abyss-data,target=/data abyss
```
5) Abyss should now be running! If you have any issues, feel free to join [the support server.](https://discord.gg/RsRps9M)
   
   
### Updating
If you are using my [bash script for updating](https://github.com/abyssal512/abyss-scripts/blob/master/update-abyss), this is just a simple execution of `./update-abyss`. Otherwise, view how that script works, and run your equivalent on your terminal.
