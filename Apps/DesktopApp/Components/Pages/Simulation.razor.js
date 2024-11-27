import * as THREE from 'three';
import Stats from 'three/addons/libs/stats.module.js';
import { Sky } from 'three/addons/objects/Sky.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { MTLLoader } from 'three/addons/loaders/MTLLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

const levelScale = 1500;

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
        },
        visibleTrucks: {},
    };

    // load models
    window.simulationView.models.truck_01 = await loadModel('truck/truck_01');
    // window.simulationView.models.warehouse_01 = await loadModel('warehouse/01/scene', 'gltf');
    // window.simulationView.models.warehouse_02 = await loadModel('warehouse/02/scene', 'gltf');

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

async function initializeWorld() {
    const asphalt_02_diff = await loadTexture('grass/01', 'png');
    // const asphalt_02_disp = await loadTexture('asphalt_02_disp_4k', 'png');
    // const asphalt_02_rough = await loadTexture('asphalt_02_rough_4k');

    const harborGroundPlane = new THREE.PlaneGeometry(levelScale, levelScale);
    const harborGroundMaterial = new THREE.MeshBasicMaterial({
        map: asphalt_02_diff,
        // bumpMap: asphalt_02_rough,
        // displacementMap: asphalt_02_disp,
        side: THREE.DoubleSide
    });
    const harborGround = new THREE.Mesh(harborGroundPlane, harborGroundMaterial);

    harborGroundPlane.rotateX(1.5707963);
    harborGround.position.x = levelScale / 2;
    harborGround.position.y = -1;
    harborGround.position.z = levelScale / 2;

    window.simulationView.scene.add(harborGround);
}

export async function removeTruck(truckId) {
    if (!isInitialized()) {
        return;
    }
    
   if (!window.simulationView.visibleTrucks.hasOwnProperty(truckId)) {
       return;
   }

    window.simulationView.scene.remove(window.simulationView.visibleTrucks[truckId].model);
    window.simulationView.visibleTrucks[truckId] = undefined;
}

export async function addTruck(id, locationX, locationY) {
    if (!isInitialized()) {
        return;
    }

    const hasTruck = window.simulationView.visibleTrucks.hasOwnProperty(id);
    if (!hasTruck) {
        let model = window.simulationView.models.truck_01.clone();
        window.simulationView.visibleTrucks[id] = {
            id: id,
            model: model,
        }

        window.simulationView.scene.add(model);
    }
    
    const truck = window.simulationView.visibleTrucks[id];
    truck.model.position.x = locationX;
    truck.model.position.y = 1;
    truck.model.position.z = locationY;
    truck.model.needsUpdate = true;
    
    if (!hasTruck) {
        console.log(truck);
    }
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
    camera.position.set( 0, 40, -50 );
    window.simulationView.camera = camera;

    // setup controls
    let controls = new OrbitControls( camera, renderer.domElement );
    controls.maxPolarAngle = Math.PI * 0.495;
    controls.target.set( 0, 0, 0 );
    controls.minDistance = 10.0;
    controls.maxDistance = levelScale / 12;
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
            texture.repeat.set( 8, 2 );
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
