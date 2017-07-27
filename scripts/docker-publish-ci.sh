#!/bin/bash
DOCKER_ENV=''
DOCKER_TAG=''
case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_ENV=production
    DOCKER_TAG=latest
    ;;
  "develop")
    DOCKER_ENV=development
    DOCKER_TAG=dev
    ;;    
esac

docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD
docker build -f ./src/Collectively.Services.Users/Dockerfile.$DOCKER_ENV -t collectively.services.users:$DOCKER_TAG ./src/Collectively.Services.Users
docker tag collectively.services.users:$DOCKER_TAG $DOCKER_USERNAME/collectively.services.users:$DOCKER_TAG
docker push $DOCKER_USERNAME/collectively.services.users:$DOCKER_TAG