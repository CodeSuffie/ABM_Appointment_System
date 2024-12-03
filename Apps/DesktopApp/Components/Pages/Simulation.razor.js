import * as THREE from 'three';
import Stats from 'three/addons/libs/stats.module.js';
import { Sky } from 'three/addons/objects/Sky.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { MTLLoader } from 'three/addons/loaders/MTLLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

const globalScale = 1;
const levelScale = 2500 * globalScale;
const scaleX = 4.5 * globalScale;
const scaleY = 4.5 * globalScale;

const truckScale = 0.25 * globalScale;
const wheelStopperScale = 0.0075 * globalScale;
const rollingShutterScale = 1 * globalScale;
const chainLinkFenceScale = 0.55 * globalScale;

const gltfLoader = new GLTFLoader();
const objLoader = new OBJLoader();
const mtlLoader = new MTLLoader();
const textureLoader = new THREE.TextureLoader();

/* 
    INITIALIZATION
 */
function isInitialized() {
    return !(window.simulationView === undefined || window.simulationView === null);
}

async function initializeModels() {
    // load models
    window.simulationView.models.truck_01 = await loadModel('truck/truck_01');
    window.simulationView.models.wheel_stopper = await loadModel('parking/wheel_stopper');
    window.simulationView.models.rolling_shutter = await loadModel('hub/rolling_shutter');
    window.simulationView.models.chain_link_fence = await loadModel('hub/chain_link_fence');
    // window.simulationView.models.warehouse_01 = await loadModel('warehouse/01/scene', 'gltf');
    // window.simulationView.models.warehouse_02 = await loadModel('warehouse/02/scene', 'gltf');
}

async function initializeTextures() {
    // grass
    window.simulationView.textures.grass[0].color = await loadTexture('grass/01', 'png');
    
    // gravel
    window.simulationView.textures.gravel[0].color = await loadTexture('gravel/01/color', 'jpg');
    window.simulationView.textures.gravel[0].specular = await loadTexture('gravel/01/specular', 'jpg');
    window.simulationView.textures.gravel[0].bump = await loadTexture('gravel/01/bump', 'png');
    window.simulationView.textures.gravel[0].normal = await loadTexture('gravel/01/normal', 'jpg');
    window.simulationView.textures.gravel[0].displacement = await loadTexture('gravel/01/displacement', 'jpg');
    
    // metal
    window.simulationView.textures.metal[0].color = await loadTexture('metal/01/color', 'jpg');
    window.simulationView.textures.metal[0].specular = await loadTexture('metal/01/specular', 'jpg');
    window.simulationView.textures.metal[0].bump = await loadTexture('metal/01/bump', 'png');
    window.simulationView.textures.metal[0].normal = await loadTexture('metal/01/normal', 'jpg');
    window.simulationView.textures.metal[0].displacement = await loadTexture('metal/01/displacement', 'jpg');
    
    // brick
    window.simulationView.textures.brick[0].color = await loadTexture('brick/01/color', 'jpg');
    window.simulationView.textures.brick[0].specular = await loadTexture('brick/01/specular', 'jpg');
    window.simulationView.textures.brick[0].bump = await loadTexture('brick/01/bump', 'png');
    window.simulationView.textures.brick[0].normal = await loadTexture('brick/01/normal', 'jpg');
    window.simulationView.textures.brick[0].displacement = await loadTexture('brick/01/displacement', 'jpg');

    // road
    window.simulationView.textures.road[0].color = await loadTexture('road/01/color', 'jpg');
    window.simulationView.textures.road[0].specular = await loadTexture('road/01/specular', 'jpg');
    window.simulationView.textures.road[0].bump = await loadTexture('road/01/bump', 'png');
    window.simulationView.textures.road[0].normal = await loadTexture('road/01/normal', 'jpg');
    window.simulationView.textures.road[0].displacement = await loadTexture('road/01/displacement', 'jpg');
}

export async function initialize() {
    // setup state
    window.simulationView = {
        domContainer: null,
        scene: null,
        light: null,
        sun: null,
        sky: null,
        camera: null,
        renderer: null,
        controls: null,
        models: {
            truck_01: null,
            warehouse_01: null,
            warehouse_02: null,
            wheel_stopper: null,
            rolling_shutter: null,
            chain_link_fence : null,
        },
        textures: {
            grass: [
                {
                    color: null,
                }
            ],
            gravel: [
                {
                    color: null,
                    specular: null,
                    bump: null,
                    normal: null,
                    displacement: null,
                }
            ],
            metal: [
                {
                    color: null,
                    specular: null,
                    bump: null,
                    normal: null,
                    displacement: null,
                }
            ],
            brick: [
                {
                    color: null,
                    specular: null,
                    bump: null,
                    normal: null,
                    displacement: null,
                }
            ],
            road: [
                {
                    color: null,
                    specular: null,
                    bump: null,
                    normal: null,
                    displacement: null
                }
            ]
        },
        visibleTrucks: {},
        parkingSpots: {},
        bays: {},
    };

    await initializeModels();
    await initializeTextures();
    
    window.simulationView.domContainer = document.getElementById( 'container' );
    createRenderer();

    const renderer = window.simulationView.renderer;

    // setup window events
    window.addEventListener( 'resize', onWindowResize );

    const scene = new THREE.Scene();
    scene.fog = new THREE.Fog(0x606060, 1, (levelScale / 4) * 3);
    scene.perObjectFrustumCulled = true;
    window.simulationView.scene = scene;

    initializeWorld();

    // add ambient light
    const light = new THREE.AmbientLight( 0x808080, 1 ); // soft white light
    scene.add( light );
    window.simulationView.light = light;

    // setup sun
    const sun = new THREE.Vector3();
    window.simulationView.sun = sun;

    // setup skybox
    const sky = new Sky();
    sky.scale.setScalar( levelScale );
    window.simulationView.sky = sky;

    const skyUniforms = sky.material.uniforms;
    skyUniforms[ 'turbidity' ].value = 10;
    skyUniforms[ 'rayleigh' ].value = 3;
    skyUniforms[ 'mieCoefficient' ].value = 0.01;
    skyUniforms[ 'mieDirectionalG' ].value = 0.7;

    const pmremGenerator = new THREE.PMREMGenerator( renderer );
    const sceneEnv = new THREE.Scene();

    let renderTarget;

    // update sun parameters
    function updateSun() {
        const phi = THREE.MathUtils.degToRad( 90 - 2 );
        const theta = THREE.MathUtils.degToRad( 90 );

        sun.setFromSphericalCoords( 1, phi, theta );

        sky.material.uniforms[ 'sunPosition' ].value.copy( sun );

        if ( renderTarget !== undefined ) {
            renderTarget.dispose();
        }

        sceneEnv.add(sky);
        renderTarget = pmremGenerator.fromScene( sceneEnv );
        scene.add(sky);

        scene.environment = renderTarget.texture;
    }
    updateSun();

    // start rendering
    requestAnimationFrame(onFrame);
}

function createMaterial(materialData, sizeX, sizeY) {
    // clone textures
    const map = materialData.hasOwnProperty('color') ? materialData.color.clone() : undefined;
    const specularMap = materialData.hasOwnProperty('specular') ? materialData.specular.clone() : undefined;
    const bumpMap = materialData.hasOwnProperty('bump') ? materialData.bump.clone() : undefined;
    const normalMap = materialData.hasOwnProperty('normal') ? materialData.normal.clone() : undefined;
    const displacementMap = materialData.hasOwnProperty('displacement') ? materialData.displacement.clone() : undefined;

    // repeat based on X/Y size
    map?.repeat.set(sizeX, sizeY);
    specularMap?.repeat.set(sizeX, sizeY);
    bumpMap?.repeat.set(sizeX, sizeY);
    normalMap?.repeat.set(sizeX, sizeY);
    displacementMap?.repeat.set(sizeX, sizeY);

    // construct material
    return new THREE.MeshBasicMaterial({
        map: map,
        specularMap: specularMap,
        bumpMap: bumpMap,
        normalMap: normalMap,
        displacementMap: displacementMap,
        side: THREE.DoubleSide
    });
}

function createVerticalMaterialPlane(materialData, sizeX, sizeY) {
    const material = createMaterial(materialData, sizeX, sizeY);
    const plane = new THREE.PlaneGeometry(sizeX * scaleX, sizeY * scaleY);
    const planeMesh = new THREE.Mesh(plane, material);
    
    return planeMesh;
}

function createHorizontalMaterialPlane(materialData, sizeX, sizeY) {
    const material = createMaterial(materialData, sizeX, sizeY);
    const plane = new THREE.PlaneGeometry(sizeX * scaleX, sizeY * scaleY);
    const planeMesh = new THREE.Mesh(plane, material);

    plane.rotateX(1.5707963);
    return planeMesh;
}

function createGrassPlane(sizeX, sizeY, variant, horizontal) {
    if (variant === null || variant === undefined || variant >= window.simulationView.textures.grass.length) {
        variant = 0;
    }

    if (horizontal === null || horizontal === undefined) {
        horizontal = true;
    }

    const material = window.simulationView.textures.grass[variant];
    if (horizontal) {
        return createHorizontalMaterialPlane(material, sizeX, sizeY);
    }

    return createVerticalMaterialPlane(material, sizeX, sizeY);
}

function createGravelPlane(sizeX, sizeY, variant, horizontal) {
    if (variant === null || variant === undefined || variant >= window.simulationView.textures.gravel.length) {
        variant = 0;
    }

    if (horizontal === null || horizontal === undefined) {
        horizontal = true;
    }

    const material = window.simulationView.textures.gravel[variant];
    if (horizontal) {
        return createHorizontalMaterialPlane(material, sizeX, sizeY);
    }
        
    return createVerticalMaterialPlane(material, sizeX, sizeY);
}

function createMetalPlane(sizeX, sizeY, variant, horizontal) {
    if (variant === null || variant === undefined || variant >= window.simulationView.textures.metal.length) {
        variant = 0;
    }

    if (horizontal === null || horizontal === undefined) {
        horizontal = true;
    }

    const material = window.simulationView.textures.metal[variant];
    if (horizontal) {
        return createHorizontalMaterialPlane(material, sizeX, sizeY);
    }

    return createVerticalMaterialPlane(material, sizeX, sizeY);
}

function createBrickPlane(sizeX, sizeY, variant, horizontal) {
    if (variant === null || variant === undefined || variant >= window.simulationView.textures.brick.length) {
        variant = 0;
    }
    
    if (horizontal === null || horizontal === undefined) {
        horizontal = true;
    }

    const material = window.simulationView.textures.brick[variant];
    if (horizontal) {
        return createHorizontalMaterialPlane(material, sizeX, sizeY);
    }

    return createVerticalMaterialPlane(material, sizeX, sizeY);
}

async function initializeWorld() {
    const plane = createGrassPlane(levelScale, levelScale);
    plane.position.x = levelScale / 2;
    plane.position.y = -0.5;
    plane.position.z = levelScale / 2;
    window.simulationView.scene.add(plane);
}

export function addBay(id, locationX, locationY, sizeX, sizeY) {
    if (!isInitialized()) {
        return;
    }

    if (window.simulationView.bays.hasOwnProperty(id)) {
        return;
    }

    const ground = createGravelPlane(sizeX, sizeY);
    ground.position.x = locationX * scaleX;
    ground.position.y = -0.49;
    ground.position.z = locationY * scaleY;
    window.simulationView.scene.add(ground);

    const model = window.simulationView.models.rolling_shutter.clone();
    model.position.x = locationX * scaleX;
    model.position.y = -0.49;
    model.position.z = ((locationY * scaleY) - (scaleY / 2)) + 0.2;
    model.scale.set(rollingShutterScale, rollingShutterScale, rollingShutterScale);
    window.simulationView.scene.add(model);
    
    window.simulationView.bays[id] = {
        id: id,
        plane: ground,
    };
}

function addFence(locationX, locationY, onXAxis) {
    const model = window.simulationView.models.chain_link_fence.clone();

    model.position.y = -(1 * globalScale);

    if (onXAxis) {
        model.position.x = ((locationX * scaleX)) - (0.625 * globalScale);
        model.position.z = ((locationY * scaleY) - scaleY) + (1 * globalScale);
    } else {
        model.position.x = ((locationX * scaleX) - scaleX) + (1 * globalScale);
        model.position.z = ((locationY * scaleY)) + (0.625 * globalScale);
        model.rotateY(1.5707963);
    }

    model.scale.set(chainLinkFenceScale, chainLinkFenceScale, chainLinkFenceScale);
    window.simulationView.scene.add(model);
}

export function addBoundaries(id, minX, maxX, minY, maxY) {
    var sizeX = maxX - minX;
    var sizeY = maxY - minY;
    
    for (var i = 0; i < sizeX + 1; i++) {
        addFence(minX + i, minY, true);
        addFence(minX + i, maxY + 1, true);
    }
    
    for (var i = 0; i < sizeY + 1; i++) {
        addFence(minX, minY + i, false);
        addFence(maxX + 1, minY + i, false);
    }
}

function addWall(locationX, locationY, sizeX, sizeY, onXAxis) {
    const plane = createBrickPlane(sizeX, sizeY, 0, false);

    plane.position.y = -0.49;
    if (onXAxis) {
        plane.position.x = (locationX * scaleX) + scaleX;
        plane.position.z = (locationY * scaleY) - ((sizeX * scaleY) / 2);
    } else {
        plane.position.x = (locationX * scaleX) - (scaleX / 2);
        plane.position.z = (locationY * scaleY) - (sizeX * scaleY);
        plane.rotateY(1.5707963);
    }
    
    window.simulationView.scene.add(plane);
}

function addRoof(locationX, locationY, sizeX, sizeY) {
    const plane = createMetalPlane(sizeX, sizeY);

    plane.position.x = (locationX * scaleX) + scaleX;
    plane.position.y = 1.6;
    plane.position.z = (locationY * scaleY) - scaleY;

    window.simulationView.scene.add(plane);
}

export function addHub(id, minX, maxX, minY, maxY) {
    var sizeX = maxX - minX;
    var sizeY = maxY - minY;
    
    addWall(minX, minY, sizeX, 1, true);
    addWall(minX, maxY, sizeX, 1, true);
    addWall(minX, minY, sizeY, 1, false);
    addWall(maxX, minY, sizeY, 1, false);

    addRoof(minX, minY, sizeX, sizeY);
}

export function addParkingSpot(id, locationX, locationY, sizeX, sizeY) {
    if (!isInitialized()) {
        return;
    }

    if (window.simulationView.parkingSpots.hasOwnProperty(id)) {
        return;
    }
    
    const ground = createGravelPlane(sizeX, sizeY);
    ground.position.x = locationX * scaleX;
    ground.position.y = -0.49;
    ground.position.z = locationY * scaleY;
    window.simulationView.scene.add(ground);

    const model = window.simulationView.models.wheel_stopper.clone();
    model.position.x = locationX * scaleX;
    model.position.y = -0.49;
    model.position.z = (locationY * scaleY) + ((scaleY / 2) - 0.15);
    model.scale.set(wheelStopperScale / 2, wheelStopperScale / 3, wheelStopperScale / 4);
    window.simulationView.scene.add(model);
    
    window.simulationView.parkingSpots[id] = {
        id: id,
        plane: ground,
        model: model,
    };
}

export function removeTruck(id) {
    if (!isInitialized()) {
        return;
    }
    
   if (!window.simulationView.visibleTrucks.hasOwnProperty(id)) {
       return;
   }

    window.simulationView.visibleTrucks[id].model.removeFromParent();
    window.simulationView.visibleTrucks[id].model = undefined;
    delete window.simulationView.visibleTrucks[id];
}

export function addTruck(id, locationX, locationY) {
    if (!isInitialized()) {
        return;
    }

    locationX *= scaleX;
    locationY *= scaleY;

    const hasTruck = window.simulationView.visibleTrucks.hasOwnProperty(id);
    if (!hasTruck) {
        let model = window.simulationView.models.truck_01.clone();
        model.scale.set(truckScale, truckScale, truckScale);
        window.simulationView.scene.add(model);

        window.simulationView.visibleTrucks[id] = {
            id: id,
            model: model,
        }
    }
    
    const truck = window.simulationView.visibleTrucks[id];
    if (hasTruck) {
        truck.model.lookAt(locationX, 0, locationY);
    }
    
    truck.model.position.x = locationX;
    truck.model.position.y = 0;
    truck.model.position.z = locationY;
    truck.model.needsUpdate = true;
}

function createRenderer() {
    const container = window.simulationView.domContainer;

    // setup renderer
    let renderer = new THREE.WebGLRenderer({
        antialias: false,
        powerPreference: "high-performance",
    });
    renderer.setPixelRatio( window.devicePixelRatio );
    renderer.setSize( container.offsetWidth, container.offsetHeight );
    // r.toneMapping = THREE.ACESFilmicToneMapping;
    // r.toneMappingExposure = 0.5;
    renderer.sortObjects = false;
    renderer.occlusionCulling = true;
    renderer.shadowMap.enabled = false;
    renderer.shadowMap.autoUpdate = false;
    renderer.state.buffers.depth.setMask( true );
    window.simulationView.renderer = renderer;

    // setup camera
    let camera = new THREE.PerspectiveCamera( 65, container.offsetWidth / container.offsetHeight, 1, (levelScale / 6) * 3 );
    camera.position.set( 400, 50, 400 );
    window.simulationView.camera = camera;

    // setup controls
    let controls = new OrbitControls( camera, renderer.domElement );
    controls.maxPolarAngle = Math.PI * 0.495;
    controls.target.set( 450, 1, 450 );
    controls.minDistance = 0.0;
    controls.maxDistance = levelScale;
    controls.update();
    window.simulationView.controls = controls;

    // add renderer to DOM
    container.appendChild( renderer.domElement );
}

/*
    FRAME STUFF
 */
function onFrame() {
    if (!isInitialized()) {
        return;
    }

    requestAnimationFrame(onFrame);
    
    window.simulationView.renderer.render( window.simulationView.scene, window.simulationView.camera );
}

/*
    TEXTURE/MODEL LOADING
*/
async function loadModel(modelName, extension) {
    if (extension === null || extension === undefined) {
        extension = 'glb';
    }

    const modelPath = `/3d/models/${modelName}.${extension}`;
    let loader = undefined;
    if (extension === 'glb' || extension === 'gltf') {
        return new Promise((resolve, reject) => {
            gltfLoader.load(modelPath, model => {
                // model.scene.children.forEach(mesh => {
                //     mesh.occluder = true;
                //     mesh.castShadow = false;
                //     mesh.receiveShadow = false;
                //     mesh.matrixAutoUpdate = false;
                //     mesh.matrixWorldAutoUpdate = false;
                // });
                resolve(model.scene);
            }, undefined, error => {
                reject(error);
            });
        });
    } else if (extension === 'obj') {
        return new Promise((resolve, reject) => {
            objLoader.load(modelPath, model => {
                // model.children.forEach(mesh => {
                //     mesh.occluder = true;
                //     mesh.castShadow = false;
                //     mesh.receiveShadow = false;
                //     mesh.matrixAutoUpdate = false;
                //     mesh.matrixWorldAutoUpdate = false;
                // });
                resolve(model);
            }, undefined, error => {
                reject(error);
            });
        });
    } else {
        throw "oof";
    }
}

async function loadTexture(textureName, extension) {
    if (extension === null || extension === undefined) {
        extension = 'jpg';
    }

    return new Promise((resolve, reject) => {
        textureLoader.load(`/3d/textures/${textureName}.${extension}`, texture => {
            texture.wrapS = texture.wrapT = THREE.RepeatWrapping;
            resolve(texture);
        }, undefined, error => {
            reject(error);
        });
    });
}

/*
    EVENTS
 */
function onWindowResize() {
    const camera = window.simulationView.camera;
    const renderer = window.simulationView.renderer;
    const container = window.simulationView.domContainer;

    if (camera === null || camera === undefined) {
        return;
    }

    if (renderer === null || renderer === undefined) {
        return;
    }

    camera.aspect = container.offsetWidth / container.offsetHeight;
    camera.updateProjectionMatrix();
    renderer.setSize( container.offsetWidth, container.offsetHeight);
}


/*
    CLEANUP
 */
export function dispose() {
    if (!isInitialized()) {
        return;
    }

    // dispose 3js objects
    window.simulationView.light.dispose();
    window.simulationView.renderer.dispose();

    // remove all stored data
    window.simulationView = undefined;
}
