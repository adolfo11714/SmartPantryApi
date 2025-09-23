FROM mysql:8.0

# Default envs can be overridden via compose
ENV MYSQL_DATABASE=food_pantry \
    MYSQL_USER=food_user \
    MYSQL_PASSWORD=food_pass \
    MYSQL_ROOT_PASSWORD=rootpass

# Copy initialization scripts (executed on first container startup)
COPY init /docker-entrypoint-initdb.d

EXPOSE 3306
