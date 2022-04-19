docker build -t ekogroszek -f Dockerfile .

docker run -d --name ekogroszek -it --rm  ekogroszek

docker container logs ekogroszek