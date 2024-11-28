import * as THREE from 'three';
import Stats from 'three/addons/libs/stats.module.js';
import { Sky } from 'three/addons/objects/Sky.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { MTLLoader } from 'three/addons/loaders/MTLLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

const levelScale = 2500;
const scaleX = 4.5;
const scaleY = 4.5;

const truckScale = 0.25;
const wheelStopperScale = 0.0075;

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
    // window.simulationView.models.warehouse_01 = await loadModel('warehouse/01/scene', 'gltf');
    // window.simulationView.models.warehouse_02 = await loadModel('warehouse/02/scene', 'gltf');
}

async function initializeTextures() {
    // load textures
    window.simulationView.textures.grass.color = await loadTexture('grass/01', 'png');
    
    // gravel
    window.simulationView.textures.gravel.color = await loadTexture('gravel/01/color', 'jpg');
    window.simulationView.textures.gravel.specular = await loadTexture('gravel/01/specular', 'jpg');
    window.simulationView.textures.gravel.bump = await loadTexture('gravel/01/bump', 'png');
    window.simulationView.textures.gravel.normal = await loadTexture('gravel/01/normal', 'jpg');
    window.simulationView.textures.gravel.displacement = await loadTexture('gravel/01/displacement', 'jpg');
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
        },
        textures: {
            grass: {
                color: null,
            },
            gravel: {
                color: null,
                specular: null,
                bump: null,
                normal: null,
                displacement: null,
            }
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

function createHorizontalMaterialPlane(material, sizeX, sizeY) {
    const plane = new THREE.PlaneGeometry(sizeX * scaleX, sizeY * scaleY);
    const planeMesh = new THREE.Mesh(plane, material);

    plane.rotateX(1.5707963);
    return planeMesh;
}

function createGrassPlane(sizeX, sizeY) {
    // clone textures
    const map = window.simulationView.textures.grass.color.clone();

    // repeat based on X/Y size
    map.repeat.set(sizeX, sizeY);

    // construct material
    const material = new THREE.MeshBasicMaterial({
        map: map,
        side: THREE.DoubleSide
    });
    
    return createHorizontalMaterialPlane(material, sizeX, sizeY);
}

function createGravelPlane(sizeX, sizeY) {
    // clone textures
    const map = window.simulationView.textures.gravel.color.clone();
    const specularMap = window.simulationView.textures.gravel.specular.clone();
    const bumpMap = window.simulationView.textures.gravel.bump.clone();
    const normalMap = window.simulationView.textures.gravel.normal.clone();
    const displacementMap = window.simulationView.textures.gravel.displacement.clone();
    
    // repeat based on X/Y size
    map.repeat.set(sizeX, sizeY);
    specularMap.repeat.set(sizeX, sizeY);
    bumpMap.repeat.set(sizeX, sizeY);
    normalMap.repeat.set(sizeX, sizeY);
    displacementMap.repeat.set(sizeX, sizeY);
    
    // construct material
    const material = new THREE.MeshBasicMaterial({
        map: map,
        specularMap: specularMap,
        bumpMap: bumpMap,
        normalMap: normalMap,
        displacementMap: displacementMap,
        side: THREE.DoubleSide
    });

    return createHorizontalMaterialPlane(material, sizeX, sizeY);
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

    const groundPlane = new THREE.PlaneGeometry(sizeX * scaleX, sizeY * scaleY);
    const groundMaterial = new THREE.MeshPhongMaterial({
        side: THREE.DoubleSide
    });
    groundMaterial.color.setHSL(0, 1, .5);  // red
    groundMaterial.flatShading = true;
    const ground = new THREE.Mesh(groundPlane, groundMaterial);

    groundPlane.rotateX(1.5707963);
    ground.position.x = locationX * scaleX;
    ground.position.y = -0.49;
    ground.position.z = locationY * scaleY;

    window.simulationView.scene.add(ground);
    window.simulationView.bays[id] = {
        id: id,
        plane: ground,
    };
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
    model.position.z = (locationY * scaleY) + ((scaleY / 2) - 2);
    model.scale.set(wheelStopperScale, wheelStopperScale / 3, wheelStopperScale / 4);
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
    camera.position.set( 0, 40, 0 );
    window.simulationView.camera = camera;

    // setup controls
    let controls = new OrbitControls( camera, renderer.domElement );
    controls.maxPolarAngle = Math.PI * 0.495;
    controls.target.set( 100, 1, 100 );
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
