FROM mono:6.8.0.96

ENV GODOT_ENGINE_FOLDER=Godot_v3.2-stable_mono_linux_headless_64
ENV GODOT_ENGINE_URL=https://downloads.tuxfamily.org/godotengine/3.2/mono/${GODOT_ENGINE_FOLDER}.zip
ENV GODOT_EXPORTS_URL=https://downloads.tuxfamily.org/godotengine/3.2/mono/Godot_v3.2-stable_mono_export_templates.tpz
ENV EXPORTS_TEMPLATE_PATH=/root/.local/share/godot/templates/3.2.stable.mono
ENV GODOT_BIN=/usr/local/godot/godot

WORKDIR /usr/local

RUN echo "::[ Installing APT packages ... ]" \
    && apt-get update && apt-get install -y wget unzip

RUN echo "::[ Downloading Godot Engine ... ]" \
    && wget -q $GODOT_ENGINE_URL \
    && echo "::[ Downloading Godot Engine export templates ... ]" \
    && wget -q $GODOT_EXPORTS_URL \
    && echo "::[ Unzipping files ... ]" \
    && mkdir -p $EXPORTS_TEMPLATE_PATH \
    && unzip -q *.zip \
    && unzip -q *.tpz \
    && rm *.zip *.tpz \
    && mv templates/* $EXPORTS_TEMPLATE_PATH \
    && rm -r templates \
    && mv $GODOT_ENGINE_FOLDER godot \
    && cd godot \
    && mv $(echo $GODOT_ENGINE_FOLDER | sed s/_64/\.64/g) godot

WORKDIR /code

ENTRYPOINT [ "/usr/local/godot/godot" ]