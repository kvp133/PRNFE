// Global variables
let roles = []
let permissions = []
let users = []
let currentRole = null
let selectedPermissions = []
let selectedUsers = []
let currentPage = "dashboard"
const bootstrap = window.bootstrap

// Initialize page
document.addEventListener("DOMContentLoaded", () => {
    initializePage()
    setupEventListeners()
})

function initializePage() {
    // Determine current page from URL
    const path = window.location.pathname
    if (path.includes("UserManagement")) {
        currentPage = "users"
        loadUserManagement()
    } else if (path.includes("PermissionManagement")) {
        currentPage = "permissions"
        loadPermissionManagement()
    }

    // Set active nav link
    updateActiveNavLink()
}

function setupEventListeners() {
    // Navigation links
    document.getElementById("userManagementLink").addEventListener("click", (e) => {
        e.preventDefault()
        loadUserManagement()
        history.pushState(null, "", "/Admin/UserManagement")
        currentPage = "users"
        updateActiveNavLink()
    })

    document.getElementById("permissionManagementLink").addEventListener("click", (e) => {
        e.preventDefault()
        loadPermissionManagement()
        history.pushState(null, "", "/Admin/PermissionManagement")
        currentPage = "permissions"
        updateActiveNavLink()
    })
}

function updateActiveNavLink() {
    // Remove active class from all nav links
    document.querySelectorAll(".nav-link").forEach((link) => {
        link.classList.remove("active")
    })

    // Add active class to current page
    if (currentPage === "users") {
        document.getElementById("userManagementLink").classList.add("active")
    } else if (currentPage === "permissions") {
        document.getElementById("permissionManagementLink").classList.add("active")
    }
}

// API Functions
async function apiCall(url, method = "GET", data = null) {
    try {
        showLoading()

        // Lấy token từ cookie
        const token = getCookie("AccessToken")
        
        const options = {
            method: method,
            headers: {
                "Content-Type": "application/json",
                RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]')?.value,
            },
        }

        // Thêm Authorization header nếu có token
        if (token) {
            options.headers["Authorization"] = `Bearer ${token}`
        }

        if (data && method !== "GET") {
            options.body = JSON.stringify(data)
        }

        const response = await fetch(url, options)
        const result = await response.json()

        hideLoading()
        return result
    } catch (error) {
        hideLoading()
        showAlert("Có lỗi xảy ra: " + error.message, "danger")
        return { success: false, message: error.message }
    }
}

// Helper function để lấy cookie
function getCookie(name) {
    const value = `; ${document.cookie}`
    const parts = value.split(`; ${name}=`)
    if (parts.length === 2) return parts.pop().split(';').shift()
    return null
}



// Load User Management Page
async function loadUserManagement() {
    const content = `
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h2 class="admin-title">Quản lý người dùng</h2>
                <p class="text-muted">Quản lý roles và user assignments</p>
            </div>
            <button class="btn btn-gradient" onclick="openRoleModal()">
                <i class="fas fa-plus me-2"></i>
                Thêm Role mới
            </button>
        </div>

        <div class="glass-card p-4">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h5 class="mb-0">Danh sách Roles</h5>
                <div class="d-flex gap-2">
                    <input type="text" id="roleSearchInput" class="form-control glass-input" placeholder="Tìm kiếm role..." style="width: 250px;" onkeyup="searchRoles()">
                    <button class="btn btn-outline-glass" onclick="loadRoles()">
                        <i class="fas fa-refresh"></i>
                    </button>
                </div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover glass-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Tên Role</th>
                            <th>Mô tả</th>
                            <th>Số Users</th>
                            <th>Số Permissions</th>
                            <th>Trạng thái</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody id="rolesTableBody">
                        <!-- Roles will be loaded here -->
                    </tbody>
                </table>
            </div>
        </div>
    `

    document.getElementById("contentArea").innerHTML = content
    await loadRoles()
}

// Load Permission Management Page
async function loadPermissionManagement() {
    const content = `
        <div class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h2 class="admin-title">Quản lý quyền</h2>
                <p class="text-muted">Quản lý permissions và role assignments</p>
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <div class="glass-card p-4 mb-4">
                    <h5>Danh sách Permissions</h5>
                    <div class="mb-3">
                        <input type="text" id="permissionSearchList" class="form-control glass-input" placeholder="Tìm kiếm permission..." onkeyup="filterPermissionsList()">
                    </div>
                    <div id="permissionsList" class="permission-list" style="max-height: 500px; overflow-y: auto;">
                        <!-- Permissions will be loaded here -->
                    </div>
                </div>
            </div>
            
            <div class="col-md-8">
                <div class="glass-card p-4">
                    <h5 class="text-center mb-4">Role Permission Matrix</h5>
                    <div class="table-responsive">
                        <table class="table glass-table" id="permissionMatrix">
                            <thead>
                                <tr id="matrixHeader">
                                    <th style="width: 40%;">Permission</th>
                                    <!-- Role headers will be added dynamically -->
                                </tr>
                            </thead>
                            <tbody id="permissionMatrixBody">
                                <!-- Permission matrix will be loaded here -->
                            </tbody>
                        </table>
                    </div>
                    <div class="text-center">
                        <button class="btn matrix-save-btn" onclick="savePermissionMatrix()">
                            <i class="fas fa-save me-2"></i>
                            Lưu thay đổi Matrix
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `

    document.getElementById("contentArea").innerHTML = content
    await loadPermissions()
    await loadRoles()
    await buildPermissionMatrix()
}

// Data Loading Functions
async function loadRoles() {
    const result = await apiCall("/Admin/GetRoles")
    if (result.success) {
        roles = (result.data || []).map(role => ({
            ...role,
            permissions: Array.isArray(role.permissions) ? role.permissions : []
        }))
        renderRolesTable()
    } else {
        showAlert("Không thể tải danh sách roles: " + result.message, "danger")
    }
}

async function loadPermissions() {
    const result = await apiCall("/Admin/GetPermissions")
    if (result.success) {
        permissions = result.data || []
        renderPermissionsList()
    } else {
        showAlert("Không thể tải danh sách permissions: " + result.message, "danger")
    }
}

async function loadRoleUsers(roleId) {
    const result = await apiCall(`/Admin/GetRoleUsers/${roleId}`)
    if (result.success) {
        return result.data || []
    } else {
        showAlert("Không thể tải danh sách users: " + result.message, "danger")
        return []
    }
}

// Render Functions
function renderRolesTable() {
    const tbody = document.getElementById("rolesTableBody")
    if (!tbody) return

    tbody.innerHTML = ""

    roles.forEach((role) => {
        const row = document.createElement("tr")
        const permissionCount = role.permissions ? role.permissions.length : 0
        const statusText = role.isActive ? "Hoạt động" : "Không hoạt động"
        const statusClass = role.isActive ? "active" : "inactive"

        row.innerHTML = `
            <td>${role.id}</td>
            <td><span class="role-badge ${getRoleBadgeClass(role.name)}">${role.name || "N/A"}</span></td>
            <td>${role.description || "Không có mô tả"}</td>
            <td><span class="badge bg-primary">0</span></td>
            <td><span class="badge bg-success">${permissionCount}</span></td>
            <td><span class="status-badge ${statusClass}">${statusText}</span></td>
            <td>
                <div class="btn-group">
                    <button class="btn btn-sm btn-outline-primary" onclick="editRole(${role.id})" title="Sửa">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-success" onclick="managePermissions(${role.id})" title="Quản lý Permissions">
                        <i class="fas fa-key"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-info" onclick="manageUsers(${role.id})" title="Quản lý Users">
                        <i class="fas fa-users"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteRole(${role.id})" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        `
        tbody.appendChild(row)
    })
}

function getRoleBadgeClass(roleName) {
    if (!roleName) return "secondary"

    const name = roleName.toLowerCase()
    if (name.includes("admin") || name.includes("quản trị")) return "admin"
    if (name.includes("manager") || name.includes("quản lý")) return "manager"
    return "user"
}

function renderPermissionsList() {
    const container = document.getElementById("permissionsList")
    if (!container) return

    container.innerHTML = ""

    permissions.forEach((permission) => {
        const div = document.createElement("div")
        div.className = "permission-item mb-2"
        div.setAttribute("data-permission-id", permission.permissionId)

        const statusIcon = permission.flagActive ? "fas fa-check-circle text-success" : "fas fa-times-circle text-danger"

        div.innerHTML = `
            <div class="d-flex align-items-center p-3 rounded-lg backdrop-blur-sm bg-white/10 border border-white/20 hover:bg-white/20 transition-all">
                <i class="fas fa-key text-primary me-3"></i>
                <div class="flex-grow-1">
                    <div class="fw-medium">${permission.permissionName || "N/A"}</div>
                    <small class="text-muted">${permission.description || "Không có mô tả"}</small>
                    <div class="mt-1">
                        <small class="text-muted">ID: ${permission.permissionId}</small>
                    </div>
                </div>
                <i class="${statusIcon} ms-2"></i>
            </div>
        `
        container.appendChild(div)
    })
}

async function buildPermissionMatrix() {
    if (roles.length === 0 || permissions.length === 0) return

    // Build header
    const header = document.getElementById("matrixHeader")
    if (!header) return

    header.innerHTML = '<th style="width: 40%;">Permission</th>'
    roles.forEach((role) => {
        header.innerHTML += `<th style="width: ${60 / roles.length}%; text-align: center;">${role.name}</th>`
    })

    // Build matrix body
    const tbody = document.getElementById("permissionMatrixBody")
    if (!tbody) return

    tbody.innerHTML = ""

    permissions.forEach((permission) => {
        const row = document.createElement("tr")
        let rowHtml = `<td class="permission-name-cell">
        <div class="permission-title">${permission.permissionName}</div>
        <div class="permission-id">${permission.permissionId}</div>
    </td>`

        roles.forEach((role) => {
            const hasPermission = role.permissions && role.permissions.some((p) => p.permissionId === permission.permissionId)
            rowHtml += `
                <td style="text-align: center; vertical-align: middle;">
                    <input type="checkbox" class="form-check-input" 
                           data-role-id="${role.id}" 
                           data-permission-id="${permission.permissionId}" 
                           ${hasPermission ? "checked" : ""}>
                </td>
            `
        })

        row.innerHTML = rowHtml
        tbody.appendChild(row)
    })
}

// CRUD Operations
function openRoleModal(roleId = null) {
    const modal = new bootstrap.Modal(document.getElementById("roleModal"))
    const title = document.getElementById("roleModalTitle")

    if (roleId) {
        title.textContent = "Sửa Role"
        loadRoleForEdit(roleId)
    } else {
        title.textContent = "Thêm Role mới"
        document.getElementById("roleForm").reset()
        document.getElementById("roleId").value = ""
    }

    modal.show()
}

async function loadRoleForEdit(roleId) {
    const result = await apiCall(`/Admin/GetRole/${roleId}`)
    if (result.success && result.data) {
        const role = result.data
        document.getElementById("roleId").value = role.id
        document.getElementById("roleName").value = role.name || ""
        document.getElementById("roleDescription").value = role.description || ""
    }
}

async function saveRole() {
    const roleId = document.getElementById("roleId").value
    const name = document.getElementById("roleName").value.trim()
    const description = document.getElementById("roleDescription").value.trim()

    if (!name) {
        showAlert("Vui lòng nhập tên role", "warning")
        return
    }

    // Show loading state
    const saveBtn = document.querySelector("#roleModal .btn-gradient")
    const saveText = document.getElementById("saveRoleText")
    const saveSpinner = document.getElementById("saveRoleSpinner")

    saveText.textContent = "Đang lưu..."
    saveSpinner.classList.remove("d-none")
    saveBtn.disabled = true

    try {
        let result
        if (roleId) {
            // Update existing role
            result = await apiCall(`/Admin/UpdateRole/${roleId}`, "PUT", {
                name: name,
                description: description,
            })
        } else {
            // Create new role
            result = await apiCall("/Admin/CreateRole", "POST", {
                name: name,
                description: description,
            })
        }

        if (result.success) {
            showAlert(result.message || "Role đã được lưu thành công!", "success")
            bootstrap.Modal.getInstance(document.getElementById("roleModal")).hide()
            await loadRoles()
        } else {
            showAlert(result.message || "Có lỗi xảy ra khi lưu role", "danger")
        }
    } finally {
        // Reset loading state
        saveText.textContent = roleId ? "Cập nhật" : "Lưu Role"
        saveSpinner.classList.add("d-none")
        saveBtn.disabled = false
    }
}

function editRole(roleId) {
    openRoleModal(roleId)
}

async function deleteRole(roleId) {
    const role = roles.find((r) => r.id === roleId)
    if (!role) return

    if (confirm(`Bạn có chắc chắn muốn xóa role "${role.name}"?`)) {
        const result = await apiCall(`/Admin/DeleteRole/${roleId}`, "DELETE")

        if (result.success) {
            showAlert(result.message || "Role đã được xóa thành công!", "success")
            await loadRoles()
        } else {
            showAlert(result.message || "Có lỗi xảy ra khi xóa role", "danger")
        }
    }
}

// Permission Management
async function managePermissions(roleId) {
    const role = roles.find((r) => r.id === roleId)
    if (!role) return

    currentRole = role
    document.getElementById("permissionRoleName").textContent = role.name

    // Đảm bảo role.permissions là mảng
    selectedPermissions = Array.isArray(role.permissions)
        ? role.permissions.map((p) => p.permissionId)
        : []

    renderAvailablePermissions()
    renderSelectedPermissions()

    const modal = new bootstrap.Modal(document.getElementById("permissionModal"))
    modal.show()
}

function renderAvailablePermissions() {
    const container = document.getElementById("availablePermissions")
    container.innerHTML = ""

    // Thêm nút Select All/Deselect All
    const selectAllDiv = document.createElement("div")
    selectAllDiv.className = "mb-2 d-flex gap-2"
    selectAllDiv.innerHTML = `
        <button type="button" class="btn btn-sm btn-outline-success" id="selectAllPermissionsBtn">
            <i class="fas fa-check-square me-1"></i> Chọn tất cả
        </button>
        <button type="button" class="btn btn-sm btn-outline-danger" id="deselectAllPermissionsBtn">
            <i class="fas fa-square me-1"></i> Bỏ chọn tất cả
        </button>
    `
    container.appendChild(selectAllDiv)

    permissions.forEach((permission) => {
        const div = document.createElement("div")
        div.className = "form-check permission-check mb-2"
        div.innerHTML = `
            <input class="form-check-input" type="checkbox" value="${permission.permissionId}" id="perm_${permission.permissionId}" 
                   ${selectedPermissions.includes(permission.permissionId) ? "checked" : ""} 
                   onchange="togglePermission('${permission.permissionId}')">
            <label class="form-check-label" for="perm_${permission.permissionId}">
                <i class="fas fa-key text-primary me-2"></i>
                <div>
                    <div class="fw-medium">${permission.permissionName}</div>
                    <small class="text-muted">${permission.description}</small>
                </div>
            </label>
        `
        container.appendChild(div)
    })

    // Gán sự kiện cho nút select all/deselect all
    document.getElementById("selectAllPermissionsBtn").onclick = function() {
        selectedPermissions = permissions.map(p => p.permissionId)
        renderAvailablePermissions()
        renderSelectedPermissions()
    }
    document.getElementById("deselectAllPermissionsBtn").onclick = function() {
        selectedPermissions = []
        renderAvailablePermissions()
        renderSelectedPermissions()
    }
}

function renderSelectedPermissions() {
    const container = document.getElementById("selectedPermissions")
    container.innerHTML = ""

    selectedPermissions.forEach((permId) => {
        const permission = permissions.find((p) => p.permissionId === permId)
        if (permission) {
            const div = document.createElement("div")
            div.className = "permission-item mb-2"
            div.innerHTML = `
                <div class="d-flex align-items-center justify-between p-2 rounded-lg bg-green-100/20 border border-green-200/30">
                    <div class="d-flex align-items-center">
                        <i class="fas fa-key text-success me-2"></i>
                        <div>
                            <div class="fw-medium">${permission.permissionName}</div>
                            <small class="text-muted">${permission.permissionId}</small>
                        </div>
                    </div>
                    <button class="btn btn-sm btn-outline-danger" onclick="removePermission('${permission.permissionId}')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `
            container.appendChild(div)
        }
    })
}

function togglePermission(permissionId) {
    const index = selectedPermissions.indexOf(permissionId)
    if (index > -1) {
        selectedPermissions.splice(index, 1)
    } else {
        selectedPermissions.push(permissionId)
    }
    renderSelectedPermissions()
}

function removePermission(permissionId) {
    const index = selectedPermissions.indexOf(permissionId)
    if (index > -1) {
        selectedPermissions.splice(index, 1)
        renderSelectedPermissions()
        renderAvailablePermissions()
    }
}

async function savePermissions() {
    if (!currentRole) return

    const saveBtn = document.querySelector("#permissionModal .btn-gradient")
    const saveText = document.getElementById("savePermissionText")
    const saveSpinner = document.getElementById("savePermissionSpinner")

    saveText.textContent = "Đang lưu..."
    saveSpinner.classList.remove("d-none")
    saveBtn.disabled = true

    try {
        const result = await apiCall("/Admin/AssignPermissions", "POST", {
            roleId: currentRole.id,
            permissionIds: selectedPermissions,
        })

        if (result.success) {
            showAlert(result.message || "Permissions đã được cập nhật thành công!", "success")
            bootstrap.Modal.getInstance(document.getElementById("permissionModal")).hide()
            await loadRoles()
            if (currentPage === "permissions") {
                await buildPermissionMatrix()
            }
        } else {
            showAlert(result.message || "Có lỗi xảy ra khi cập nhật permissions", "danger")
        }
    } finally {
        saveText.textContent = "Lưu Permissions"
        saveSpinner.classList.add("d-none")
        saveBtn.disabled = false
    }
}

// Permission Matrix Management
async function savePermissionMatrix() {
    const matrixData = {}
    const checkboxes = document.querySelectorAll('#permissionMatrix input[type="checkbox"]')

    checkboxes.forEach((checkbox) => {
        const roleId = checkbox.getAttribute("data-role-id")
        const permissionId = checkbox.getAttribute("data-permission-id")

        if (!matrixData[roleId]) {
            matrixData[roleId] = []
        }

        if (checkbox.checked) {
            matrixData[roleId].push(permissionId)
        }
    })

    showLoading()

    try {
        // Save each role's permissions
        for (const roleId in matrixData) {
            const result = await apiCall("/Admin/AssignPermissions", "POST", {
                roleId: Number.parseInt(roleId),
                permissionIds: matrixData[roleId],
            })

            if (!result.success) {
                showAlert(`Có lỗi khi cập nhật permissions cho role ID ${roleId}: ${result.message}`, "danger")
                return
            }
        }

        showAlert("Permission matrix đã được cập nhật thành công!", "success")
        await loadRoles()
        await buildPermissionMatrix()
    } finally {
        hideLoading()
    }
}

// User Management
async function manageUsers(roleId) {
    const role = roles.find((r) => r.id === roleId)
    if (!role) return

    currentRole = role
    document.getElementById("userRoleName").textContent = role.name

    // Load users for this role
    const roleUsers = await loadRoleUsers(roleId)
    selectedUsers = roleUsers.map((u) => u.userName)
    users = roleUsers

    renderAvailableUsers()
    renderAssignedUsers()

    const modal = new bootstrap.Modal(document.getElementById("userModal"))
    modal.show()
}

function renderAvailableUsers() {
    const container = document.getElementById("availableUsers")
    container.innerHTML = ""

    users.forEach((user) => {
        const div = document.createElement("div")
        div.className = "user-item-modal mb-2"
        div.innerHTML = `
            <div class="form-check">
                <input class="form-check-input" type="checkbox" value="${user.userName}" id="user_${user.userName}" 
                       ${selectedUsers.includes(user.userName) ? "checked" : ""} 
                       onchange="toggleUser('${user.userName}')">
                <label class="form-check-label d-flex align-items-center" for="user_${user.userName}">
                    <img src="${user.avatar || "/placeholder.svg?height=30&width=30"}" class="rounded-circle me-2" alt="User" style="width: 30px; height: 30px;">
                    <div>
                        <div class="fw-medium">${user.name || user.fullName || user.userName || "N/A"}</div>
                        <small class="text-muted">${user.email || "N/A"}</small>
                    </div>
                </label>
            </div>
        `
        container.appendChild(div)
    })
}

function renderAssignedUsers() {
    const container = document.getElementById("assignedUsers")
    container.innerHTML = ""

    selectedUsers.forEach((userName) => {
        const user = users.find((u) => u.userName === userName)
        if (user) {
            const div = document.createElement("div")
            div.className = "user-item-modal mb-2"
            div.innerHTML = `
                <div class="d-flex align-items-center justify-between p-2 rounded-lg bg-green-100/20 border border-green-200/30">
                    <div class="d-flex align-items-center">
                        <img src="${user.avatar || "/placeholder.svg?height=30&width=30"}" class="rounded-circle me-2" alt="User" style="width: 30px; height: 30px;">
                        <div>
                            <div class="fw-medium">${user.name || user.fullName || user.userName || "N/A"}</div>
                            <small class="text-muted">${user.email || "N/A"}</small>
                        </div>
                    </div>
                    <button class="btn btn-sm btn-outline-danger" onclick="removeUser('${user.userName}')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `
            container.appendChild(div)
        }
    })
}

function toggleUser(userName) {
    const index = selectedUsers.indexOf(userName)
    if (index > -1) {
        selectedUsers.splice(index, 1)
    } else {
        selectedUsers.push(userName)
    }
    renderAssignedUsers()
}

function removeUser(userName) {
    const index = selectedUsers.indexOf(userName)
    if (index > -1) {
        selectedUsers.splice(index, 1)
        renderAssignedUsers()
        renderAvailableUsers()
    }
}

async function saveUserAssignments() {
    if (!currentRole) return

    const saveBtn = document.querySelector("#userModal .btn-gradient")
    const saveText = document.getElementById("saveUserText")
    const saveSpinner = document.getElementById("saveUserSpinner")

    saveText.textContent = "Đang lưu..."
    saveSpinner.classList.remove("d-none")
    saveBtn.disabled = true

    try {
        const result = await apiCall("/Admin/AssignUsers", "POST", {
            RoleId: currentRole.id,
            UserIds: selectedUsers,
        })

        if (result.success) {
            showAlert(result.message || "User assignments đã được cập nhật thành công!", "success")
            bootstrap.Modal.getInstance(document.getElementById("userModal")).hide()
            await loadRoles()
        } else {
            showAlert(result.message || "Có lỗi xảy ra khi cập nhật user assignments", "danger")
        }
    } finally {
        saveText.textContent = "Lưu thay đổi"
        saveSpinner.classList.add("d-none")
        saveBtn.disabled = false
    }
}

// Search Functions
function searchRoles() {
    const query = document.getElementById("roleSearchInput").value.toLowerCase()
    const rows = document.querySelectorAll("#rolesTableBody tr")

    rows.forEach((row) => {
        const text = row.textContent.toLowerCase()
        row.style.display = text.includes(query) ? "" : "none"
    })
}

function filterPermissions() {
    const query = document.getElementById("permissionSearch").value.toLowerCase()
    const items = document.querySelectorAll("#availablePermissions .permission-check")

    items.forEach((item) => {
        const text = item.textContent.toLowerCase()
        item.style.display = text.includes(query) ? "" : "none"
    })
}

function filterPermissionsList() {
    const query = document.getElementById("permissionSearchList").value.toLowerCase()
    const items = document.querySelectorAll("#permissionsList .permission-item")

    items.forEach((item) => {
        const text = item.textContent.toLowerCase()
        item.style.display = text.includes(query) ? "" : "none"
    })
}

function filterUsers() {
    const query = document.getElementById("userSearch").value.toLowerCase()
    const items = document.querySelectorAll("#availableUsers .user-item-modal")

    items.forEach((item) => {
        const text = item.textContent.toLowerCase()
        item.style.display = text.includes(query) ? "" : "none"
    })
}

// Utility Functions
function showLoading() {
    document.getElementById("loadingOverlay").classList.remove("d-none")
}

function hideLoading() {
    document.getElementById("loadingOverlay").classList.add("d-none")
}

function showAlert(message, type = "info") {
    const alertDiv = document.createElement("div")
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`
    alertDiv.style.cssText = "top: 20px; right: 20px; z-index: 9999; min-width: 300px;"

    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `

    document.body.appendChild(alertDiv)

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.remove()
        }
    }, 5000)
}
