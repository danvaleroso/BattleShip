let hFieldValue = "";
let selectedShipCell;
let lastOfShipCells;

function manualMode() {
    const submarine = document.querySelector("#Submarine");
    addAllEvents(submarine);

    const destroyer = document.querySelector("#Destroyer");
    addAllEvents(destroyer);

    const cruiser = document.querySelector("#Cruiser");
    addAllEvents(cruiser);

    const battleship = document.querySelector("#Battleship");
    addAllEvents(battleship);

    document.querySelector("#StartButton").disabled = true;
    activatePlayerFieldDragAndDrop();
}

function IsAllShipsDeployed() {
    const submarineCount = document.querySelector("#Submarine").dataset.count == "0";
    const destroyerCount = document.querySelector("#Destroyer").dataset.count == "0";
    const cruiserCount = document.querySelector("#Cruiser").dataset.count == "0";
    const battleshipCount = document.querySelector("#Battleship").dataset.count == "0";

    return submarineCount && destroyerCount && cruiserCount && battleshipCount;
}

function addAllEvents(ship) {
    ship.addEventListener("click", rotate);
    ship.addEventListener("mousedown", mouseDown);
    ship.addEventListener("dragstart", dragStart);
}

function rotate(e) {
    let shipContainer = e.target.parentElement;
    shipContainer.classList.toggle(shipContainer.id + "-container-vertical");

    if (shipContainer.dataset.ishorizontal == "true") {
        shipContainer.dataset.ishorizontal = "false";
    } else {
        shipContainer.dataset.ishorizontal = "true";
    }
}

function mouseDown(e) {
    selectedShipCell = e.target.dataset.value;
    lastOfShipCells = e.target.parentElement.lastElementChild.dataset.value;
}

function dragStart(e) {
    e.dataTransfer.setData("text/plain", selectedShipCell + "," + lastOfShipCells + "," + e.target.id);
}

function activatePlayerFieldDragAndDrop() {
    for (const dropCell of document.querySelectorAll(".field-cell")) {

        dropCell.addEventListener("dragover", e => {
            e.preventDefault();
        });

        dropCell.addEventListener("drop", e => {
            e.preventDefault();
            const dropData = e.dataTransfer.getData("text/plain");

            let data = dropData.split(",");
            let selectedShipCellData = parseInt(data[0]);
            let lastOfShipCellData = parseInt(data[1]);
            let shipId = data[2];

            let currentShip = document.getElementById(shipId);

            let dropCellIndex = parseInt(dropCell.id.replace(/\D/g, ''));

            let arrayOfDropCells = [];
            for (let i = 0; i <= (lastOfShipCellData); i++) {
                if (currentShip.dataset.ishorizontal == "true") {
                    let firstShipCell = dropCellIndex - selectedShipCellData;
                    arrayOfDropCells.push(firstShipCell + i);
                } else {
                    let firstShipCell = dropCellIndex - (selectedShipCellData * 10);
                    arrayOfDropCells.push(firstShipCell + (i * 10));
                }
            }

            let isShipOnLeftEdge = arrayOfDropCells.some(value => value % 10 == 0);
            let isShipOnRightEdge = arrayOfDropCells.some(value => value % 10 == 9);
            let isShipOutOfBounds = arrayOfDropCells.some(value => value < 0 || value > 99);

            let isCellTaken = arrayOfDropCells.some(value => {
                let selectedFieldCellId = "PlayerCells" + value;
                let selectedFieldCell = document.getElementById(selectedFieldCellId);

                if (selectedFieldCell.classList.contains("taken")) return true;

                return false;
            });

            const isSurroundingsTaken = arrayOfDropCells.some((value) => {
                let selectedFieldCell;

                selectedFieldCell = document.getElementById("PlayerCells" + (value - 10));
                let isUpperTaken = (value > 9) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value + 10));
                let isLowerTaken = (value < 90) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value - 1));
                let isLeftTaken = (value % 10 != 0) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value + 1));
                let isRightTaken = (value % 10 != 9) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value - 11));
                let isUpperLeftTaken = (value % 10 != 0 && value > 10) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value - 9));
                let isUpperRightTaken = (value % 10 != 9 && value > 9) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value + 9));
                let isLowerLeftTaken = (value % 10 != 0 && value < 90) ? selectedFieldCell.classList.contains("taken") : false;

                selectedFieldCell = document.getElementById("PlayerCells" + (value + 11));
                let isLowerRightTaken = (value % 10 != 9 && value < 89) ? selectedFieldCell.classList.contains("taken") : false;

                return isUpperTaken ||
                    isLowerTaken ||
                    isLeftTaken ||
                    isRightTaken ||
                    isLowerLeftTaken ||
                    isLowerRightTaken ||
                    isUpperLeftTaken ||
                    isUpperRightTaken;
            });

            let shipCount = parseInt(currentShip.dataset.count);

            if (shipCount > 0 && !(isShipOnLeftEdge && isShipOnRightEdge) && !isShipOutOfBounds && !isCellTaken && !isSurroundingsTaken) {
                arrayOfDropCells.forEach((value) => {
                    let selectedFieldCellId = "PlayerCells" + value;
                    let selectedFieldCellClass = "Player" + shipId + "-" + shipCount;

                    let selectedFieldCell = document.getElementById(selectedFieldCellId);
                    selectedFieldCell.classList.add(selectedFieldCellClass, "taken");

                    hFieldValue += selectedFieldCellId + "," + selectedFieldCellClass + ";";
                });

                shipCount--;
                currentShip.dataset.count = shipCount;
                currentShip.parentElement.lastElementChild.innerHTML = shipCount;

                if (shipCount == 0) currentShip.draggable = false;
            }

            if (IsAllShipsDeployed()) document.querySelector("#StartButton").disabled = false;

            document.getElementById("ManualShipsGenerator").value = hFieldValue;
        });
    }
}

function showPcShips() {
    let PcSubmarine = document.querySelectorAll(".PcSubmarine-1 , .PcSubmarine-2, .PcSubmarine-3, .PcSubmarine-4");
    let PcDestroyer = document.querySelectorAll(".PcDestroyer-1, .PcDestroyer-2, .PcDestroyer-3");
    let PcCruiser = document.querySelectorAll(".PcCruiser-1, .PcCruiser-2");
    let PcBattleship = document.querySelectorAll(".PcBattleship-1");

    PcSubmarine.forEach(ship => ship.style.backgroundColor = "green");
    PcDestroyer.forEach(ship => ship.style.backgroundColor = "orange");
    PcCruiser.forEach(ship => ship.style.backgroundColor = "purple");
    PcBattleship.forEach(ship => ship.style.backgroundColor = "blue");
}
