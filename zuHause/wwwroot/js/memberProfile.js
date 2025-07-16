    let inputUserImage = document.querySelector("#inputUserImage");
    let errorMessage = document.querySelector("#userUploadStatus");
    let userImageAreaImg = document.querySelector(".user-image img");
    let uploadButton = document.querySelector("#uploadButton");
    let navUserImg = document.querySelector(".user-img img");
    let submitIdMsg = document.querySelector(".submit-id-msg");



    let inputIdFront = document.querySelector("#inputIdFront");
    let inputIdBack = document.querySelector("#inputIdBack");
    let labelIdFront = document.querySelector("label[for='inputIdFront']");
    let labelIdBack = document.querySelector("label[for='inputIdBack']");
    let frontReviewStatus = document.querySelector("#inputIdFront ~ .review-status");
    let backReviewStatus = document.querySelector("#inputIdBack ~ .review-status");
    let submitIdFile = document.querySelector("#submitIdFile");

    let userImageFormData;
    let userImageFile;
    let idFrontFormData;
    let idFBackFormData;


    // 檢查上傳檔案

    function checkUploadFile(UploadTypeCode,
    ModuleCode,
    CodeCategory, errorTextDOM, eTarget, labelDOM = null) {
        console.log(UploadTypeCode);

    let imgFile = eTarget.files[0]
    let imageSize = imgFile.size;
    let allowTypes = ["image/jpeg", "image/png", "image/webp"];
    console.log(imageSize);
    let maxImageSize = 5 * 1024 * 1024;
    let ImageUploadStatus = null;
    if (!imgFile) {
        ImageUploadStatus = "NoFileSelected";
      } else {
        if (!allowTypes.includes(imgFile.type)) {
        ImageUploadStatus = "ImageTypeError";
    eTarget.value = "";
    if (labelDOM) {
            if (UploadTypeCode === "USER_ID_FRONT") {
        labelDOM.textContent = "請上傳身分證正面照片";
            } else if (UploadTypeCode === "USER_ID_BACK") {
        labelDOM.textContent = "請上傳身分證反面照片";
            }
          }
        } else if (imageSize > maxImageSize) {
        ImageUploadStatus = "ImageTooLarge";
    eTarget.value = "";
        } else {
        ImageUploadStatus = "Valid";
        }
      }


    switch (ImageUploadStatus) {
        case "NoFileSelected":
    errorTextDOM.textContent = "請選擇檔案";
    break;
    case "ImageTypeError":
    case "ImageTooLarge":
    errorTextDOM.textContent = "請上傳5MB大小內之.jpg/.png/.webp之圖片";
    break;
    case "Valid":
    errorTextDOM.textContent = "";
      }
    if (ImageUploadStatus !== "Valid") {
        return;
      }


    let formData = new FormData();
    formData.append("UploadFile", imgFile);
    formData.append("UploadTypeCode", UploadTypeCode);// 系統表類別
    formData.append("ModuleCode", ModuleCode);// 來源模組
    formData.append("CodeCategory", CodeCategory);// 系統表模組別
    return formData;
    }


    // 大頭照上傳
    inputUserImage.addEventListener("change", function (e) {
        userImageFile = e.target.files[0];
    userImageFormData = checkUploadFile("USER_IMG",
    "MemberInfo",
    "USER_UPLOAD_TYPE", errorMessage, e.target);
    uploadButton.classList.remove("d-none");
    if (userImageFile) {
        userImageAreaImg.src = URL.createObjectURL(userImageFile);
      }
    });


    uploadButton.addEventListener("click", function () {
        fetch("/Member/Upload",
            {
                method: "POST",
                body: userImageFormData, // 整包FormData傳到後端
            }).then(result => {
                console.log("上傳成功", result);

                uploadButton.classList.add("d-none");
                if (navUserImg) {
                    navUserImg.src = URL.createObjectURL(userImageFile);
                }
            })
            .catch(error => {
                console.error("上傳錯誤", error);
                errorMessage.textContent = "上傳發生錯誤，請稍後再試";
            });

    });

    // 身分認證送出資料
    submitIdFile.addEventListener("click", async function () {
        let isValid = true;
    if (inputIdFront.files.length == 0 || inputIdBack.files.length == 0) {
        submitIdMsg.textContent = "請上傳身分證正反面照片";
    isValid = false;
      } else {
        submitIdMsg.textContent = "";
      }
    if (!isValid) {
        console.log("沒通過");
    return;
      }

    try {
        let frontResult = await uploadIdFile(idFrontFormData, "USER_ID_FRONT");
    let backResult = await uploadIdFile(idFBackFormData, "USER_ID_BACK");
    submitIdMsg.classList.remove("text-danger");
    submitIdMsg.classList.add("text-success");
    submitIdMsg.textContent = "上傳成功";

      } catch (error) {
        submitIdMsg.classList.remove("text-success");
    submitIdMsg.classList.add("text-danger");
    submitIdMsg.textContent = `${error.message}，請稍後再試`;
      }

      // 改狀態
    });

    function uploadIdFile(formData, UploadTypeCode) {
      return fetch("/Member/Upload", // 回傳Promise給await
    {
        method: "POST",
    body: formData,
        }).then(result => {
        console.log(`${UploadTypeCode}上傳結果`, result);
    if (!result.ok) {
            throw new Error(`${UploadTypeCode}上傳失敗`);
          } else {
            if (UploadTypeCode === "USER_ID_FRONT") {
        frontReviewStatus.textContent = "完成上傳";
            } else if (UploadTypeCode === "USER_ID_BACK") {
        backReviewStatus.textContent = "完成上傳";
            }
          }
    return UploadTypeCode;
        });
    }


    // 身分證上傳
    let inputIdArea = document.querySelectorAll(".input-id-area");
    inputIdArea.forEach((item, index) => {
        let label = item.querySelector("label");
    let input = item.querySelector("input");
    let idErrorMsg = item.querySelector("input + .id-error-msg");
    let originalText = label.textContent;
    input.addEventListener("change", function (e) {
        submitIdMsg.textContent = "";
    idErrorMsg.textContent = "";
    let file = e.target.files[0];
    if (file) {
        label.textContent = file.name;
    if (index === 0) {
        idFrontFormData = checkUploadFile("USER_ID_FRONT",
            "MemberInfo",
            "USER_UPLOAD_TYPE", idErrorMsg, e.target, labelIdFront);
          } else if (index === 1) {
        idFBackFormData = checkUploadFile("USER_ID_BACK",
            "MemberInfo",
            "USER_UPLOAD_TYPE", idErrorMsg, e.target, labelIdBack);
          }
        } else {
        label.textContent = originalText;
        }
      });
    });



    function formatDateTime2() {
        let date = new Date();
      const pad = (n) => n.toString().padStart(2, "0"); // 格式化
    const year = date.getFullYear();
    const month = pad(date.getMonth() + 1); // getMonth()從0開始
    const day = pad(date.getDate());
    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());
    const seconds = pad(date.getSeconds());
    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;

    }